using Dominus.Application.DTOs.Payment;
using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Application.Settings.Dominus.Application.Settings;
using Dominus.Domain.Common;
using Dominus.Application.DTOs.OrderDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Enums;
using Dominus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using DomainOrder = Dominus.Domain.Entities.Order;
using RzOrder = Razorpay.Api.Order;

namespace Dominus.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;
        private readonly IColorRepository _colorRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IShippingAddressRepository _shippingRepo;

        private readonly RazorpayService _razor;
        private readonly RazorpaySettings _razorSettings;

        public OrderService(
             IOrderRepository orderRepo,
             IProductRepository productRepo,
             IColorRepository colorRepo,
             ICartRepository cartRepo,
             IShippingAddressRepository shippingRepo,
             RazorpayService razor,
             IOptions<RazorpaySettings> razorOptions)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _colorRepo = colorRepo;
            _shippingRepo = shippingRepo;
            _cartRepo = cartRepo;

            _razor = razor;
            _razorSettings = razorOptions.Value;
        }



        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return new ApiResponse<OrderDto>(400, "No order items provided");

            int uid = int.Parse(userId);

            var addresses = await _shippingRepo.GetByUserIdAsync(uid);
            var activeAddress = addresses.FirstOrDefault(a => !a.IsDeleted && a.IsActive);

            if (activeAddress == null)
                return new ApiResponse<OrderDto>(400, "No active shipping address found. Please set one before ordering.");

            string finalAddress = $"{activeAddress.AddressLine}, {activeAddress.City}, {activeAddress.State} - {activeAddress.Pincode}";

            var order = new DomainOrder
            {
                UserId = userId,
                ShippingAddress = finalAddress,
                Status = OrderStatus.PendingPayment
            };

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = await _productRepo.GetByIdWithColorsAsync(item.ProductId);

                if (product == null || product.IsDeleted)
                    return new ApiResponse<OrderDto>(404, "Product not found");

                if (!product.IsActive || product.CurrentStock <= 0)
                    return new ApiResponse<OrderDto>(400, $"{product.Name} is not available");

                if (product.CurrentStock < item.Quantity)
                    return new ApiResponse<OrderDto>(400, $"Insufficient stock for {product.Name}");

                var color = await _colorRepo.GetByIdAsync(item.ColorId);

                if (color == null || color.IsDeleted)
                    return new ApiResponse<OrderDto>(404, "Selected color not found");

                if (!color.IsActive)
                    return new ApiResponse<OrderDto>(400, "Selected color is inactive");

                var colorMappedToProduct = product.AvailableColors.Any(pc => pc.ColorId == item.ColorId && !pc.IsDeleted);
                if (!colorMappedToProduct)
                    return new ApiResponse<OrderDto>(400, $"Color not available for {product.Name}");

                product.CurrentStock -= item.Quantity;
                product.InStock = product.CurrentStock > 0;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    ColorId = color.Id,
                    Price = product.Price
                });

                total += product.Price * item.Quantity;
            }

            order.TotalAmount = total;

            // Add order and save to ensure order.Id is populated by EF (so Razor order receipt can use it).
            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();

            // Persist product stock changes
            await _productRepo.SaveChangesAsync();

            // Create Razorpay order using the DB-generated order.Id
            var razorOrder = _razor.CreateOrder(order.TotalAmount, $"ORD_{order.Id}");
            order.RazorOrderId = razorOrder["id"].ToString();
            await _orderRepo.SaveChangesAsync();

            var mapped = Map(order);
            mapped.RazorOrderId = order.RazorOrderId;
            mapped.RazorKey = _razorSettings.Key;

            return new ApiResponse<OrderDto>(200, "Order placed successfully", mapped);
        }



        public async Task<ApiResponse<List<OrderDto>>> GetMyOrdersAsync(string userId)
        {
            var orders = await _orderRepo.GetByUserIdAsync(userId);

            return new ApiResponse<List<OrderDto>>(
                200,
                "Orders fetched",
                orders.Select(Map).ToList()
            );
        }

        public async Task<ApiResponse<List<OrderDto>>> GetOrdersByProductAsync(
    int productId,
    string userId)
        {
            if (productId <= 0)
                return new ApiResponse<List<OrderDto>>(400, "Invalid product");

            var orders = await _orderRepo.GetByUserAndProductAsync(userId, productId);

            if (!orders.Any())
                return new ApiResponse<List<OrderDto>>(
                    404,
                    "No orders found for this product"
                );

            return new ApiResponse<List<OrderDto>>(
                200,
                "Orders fetched successfully",
                orders.Select(Map).ToList()
            );
        }


        private OrderDto Map(Order order) => new()
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            RazorOrderId = order.RazorOrderId,
            RazorKey = _razorSettings.Key,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",

                ProductImages = i.Product?.Images
                    .Where(img => !img.IsDeleted)
                    .Select(img => img.ImageUrl)
                    .ToList() ?? new List<string>(),

                ColorId = i.ColorId,
                ColorName = i.Color?.Name ?? "Unknown",
                HexCode = i.Color?.HexCode ?? "#000",

                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };



        public async Task<ApiResponse<object>> AdminUpdateOrderStatusAsync(
    int orderId,
    OrderStatus newStatus)
        {
            var order = await _orderRepo.GetByIdWithItemsAsync(orderId);

            if (order == null)
                return new ApiResponse<object>(404, "Order not found");

            var oldStatus = order.Status;

            // Delivered is final
            if (oldStatus == OrderStatus.Delivered)
                return new ApiResponse<object>(
                    400,
                    "Delivered orders cannot be modified"
                );

            if (oldStatus == newStatus)
                return new ApiResponse<object>(
                    400,
                    "Order already in the requested status"
                );

            // 🚨 Enforce status order
            if (!AllowedTransitions.TryGetValue(oldStatus, out var allowedNextStatuses) ||
                !allowedNextStatuses.Contains(newStatus))
            {
                return new ApiResponse<object>(
                    400,
                    $"Invalid status change from {oldStatus} to {newStatus}"
                );
            }

            // Restore stock ONLY when PendingPayment → Cancelled
            if (oldStatus == OrderStatus.PendingPayment && newStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.CurrentStock += item.Quantity;
                        product.InStock = product.CurrentStock > 0;
                    }
                }

                order.PaymentReference = null;
                order.PaidOn = null;

                await _productRepo.SaveChangesAsync();

            }



            //// Reset payment info if cancelled
            //if (newStatus == OrderStatus.Cancelled)
            //{
            //    order.PaymentReference = null;
            //    order.PaidOn = null;
            //}

            order.Status = newStatus;
            await _orderRepo.SaveChangesAsync();

            return new ApiResponse<object>(
                200,
                $"Order status changed from {oldStatus} to {newStatus}"
            );
        }


        public async Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersForAdminAsync(
    int page,
    int pageSize,
    OrderStatus? status)
        {
            // Pagination validation
            page = page <= 0 ? 1 : page;
            pageSize = pageSize switch
            {
                <= 0 => 100,
                > 100 => 1000,
                _ => pageSize
            };

            var query = _orderRepo.Query();

            // 🔍 Status filter
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<PagedResult<OrderDto>>(
                    404,
                    "No orders found"
                );
            }

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<OrderDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = orders.Select(Map).ToList()
            };

            return new ApiResponse<PagedResult<OrderDto>>(
                200,
                "Orders fetched successfully",
                result
            );
        }

       public async Task<ApiResponse<object>> CancelOrderAsync(string userId, int orderId)
{
    var order = await _orderRepo.GetByIdWithItemsAsync(orderId);

    if (order == null || order.UserId != userId)
        return new ApiResponse<object>(404, "Order not found");

    if (order.Status != OrderStatus.PendingPayment)
        return new ApiResponse<object>(400, "Only PendingPayment orders can be cancelled");

    // 🔥 RESTORE STOCK
    foreach (var item in order.Items)
    {
                var product = await _productRepo.GetByIdTrackedAsync(item.ProductId);

                if (product != null)
        {
            product.CurrentStock += item.Quantity;
            product.InStock = product.CurrentStock > 0;
        }
    }

    order.Status = OrderStatus.Cancelled;
    order.PaymentReference = null;
    order.PaidOn = null;

    await _orderRepo.SaveChangesAsync();
    await _productRepo.SaveChangesAsync();

    return new ApiResponse<object>(200, "Order cancelled successfully");
}



       
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions =
    new()
    {
        {
            OrderStatus.PendingPayment,
            new[] { OrderStatus.Cancelled, OrderStatus.Paid }
        },
        {
            OrderStatus.Paid,
            new[] { OrderStatus.Shipped }
        },
        {
            OrderStatus.Shipped,
            new[] { OrderStatus.Delivered }
        },
        {
            OrderStatus.Delivered,
            Array.Empty<OrderStatus>()
        }
    };
        public async Task<ApiResponse<object>> VerifyPaymentAsync(string userId, RazorVerifyDto dto)
        {
            var order = await _orderRepo.GetByIdWithItemsAsync(dto.OrderId);

            if (order == null || order.UserId != userId)
                return new ApiResponse<object>(404, "Order not found");

            try
            {
                _razor.Verify(dto.RazorOrderId, dto.PaymentId, dto.Signature);

                order.Status = OrderStatus.Paid;
                order.PaymentReference = dto.PaymentId;
                order.PaidOn = DateTime.UtcNow;

                await _orderRepo.SaveChangesAsync();

                return new ApiResponse<object>(200, "Payment Verified Successfully");
            }
            catch
            {
                order.Status = OrderStatus.Cancelled;
                await _orderRepo.SaveChangesAsync();

                return new ApiResponse<object>(400, "Payment Verification Failed");
            }
        }


    }
}
