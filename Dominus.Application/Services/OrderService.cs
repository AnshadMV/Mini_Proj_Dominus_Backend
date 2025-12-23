using Dominus.Application.DTOs.Payment;
using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.OrderDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Enums;
using Dominus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;

namespace Dominus.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;

        public OrderService(
            IOrderRepository orderRepo,
            IProductRepository productRepo)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(
            string userId,
            CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return new ApiResponse<OrderDto>(400, "No order items provided");

            if (string.IsNullOrWhiteSpace(dto.ShippingAddress))
                return new ApiResponse<OrderDto>(
                    400,
                    "Shipping address is required"
                );
            var cleanedAddress = dto.ShippingAddress.Trim();
    //        cleanedAddress = System.Text.RegularExpressions.Regex
    //.Replace(cleanedAddress, @"\s+", " ");

            var order = new Order
            {
                UserId = userId,
                    ShippingAddress = cleanedAddress,
                Status = OrderStatus.PendingPayment

            };

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);

                if (product == null)
                    return new ApiResponse<OrderDto>(404, "Product not found");

                if (!product.IsActive || !product.InStock)
                    return new ApiResponse<OrderDto>(400, $"{product.Name} is not available");

                if (product.CurrentStock < item.Quantity)
                    return new ApiResponse<OrderDto>(
                        400,
                        $"Insufficient stock for {product.Name}"
                    );

                //product.CurrentStock -= item.Quantity;
                //product.InStock = product.CurrentStock > 0;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = product.Price
                });

                total += product.Price * item.Quantity;
            }

            order.TotalAmount = total;

            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveChangesAsync();

            return new ApiResponse<OrderDto>(
                201,
                "Order placed successfully",
                Map(order)
            );
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


        private static OrderDto Map(Order order) => new()
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            UserId = order.UserId,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };


        public async Task<ApiResponse<object>> PayForOrderAsync(
    string userId,
    int orderId,
    PaymentDto dto)
        {
            var order = await _orderRepo.GetByIdWithItemsAsync(orderId);

            if (order == null || order.UserId != userId)
                return new ApiResponse<object>(404, "Order not found");

            if (order.Status != OrderStatus.PendingPayment)
                return new ApiResponse<object>(400, "Order already paid or invalid");

            foreach (var item in order.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);

                if (product == null || product.IsDeleted || !product.IsActive)
                    return new ApiResponse<object>(400, "Product unavailable");

                if (product.CurrentStock < item.Quantity)
                    return new ApiResponse<object>(400, $"Insufficient stock for {product.Name}");

                product.CurrentStock -= item.Quantity;
                product.InStock = product.CurrentStock > 0;
            }

            order.Status = OrderStatus.Paid;
            order.PaymentReference = dto.PaymentReference;

            await _orderRepo.SaveChangesAsync();

            return new ApiResponse<object>(
                201,
                "Payment successful, order confirmed"
            );
        }
        public async Task<ApiResponse<object>> AdminUpdateOrderStatusAsync(
    int orderId,
    OrderStatus newStatus)
        {
            var order = await _orderRepo.GetByIdWithItemsAsync(orderId);

            if (order == null)
                return new ApiResponse<object>(404, "Order not found");

            var oldStatus = order.Status;

            if (oldStatus == OrderStatus.Delivered)
                return new ApiResponse<object>(
                    400,
                    "Delivered orders cannot be modified"
                );



            if (oldStatus == newStatus)
                return new ApiResponse<object>(
                    401,
                    "Order already in the requested status"
                );

            if (
       oldStatus == OrderStatus.Paid &&
       (newStatus == OrderStatus.Cancelled ||
        newStatus == OrderStatus.PendingPayment)
   )
            {
                foreach (var item in order.Items)
                {
                    var product = await _productRepo.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.CurrentStock += item.Quantity;
                        product.InStock = true;
                    }
                }
            }
            if (oldStatus == OrderStatus.Delivered)
                return new ApiResponse<object>(
                    400,
                    "Delivered orders cannot be modified"
                );
            if (newStatus == OrderStatus.PendingPayment)
            {
                order.PaymentReference = null;
                order.PaidOn = null;
            }
            order.Status = newStatus;

            await _orderRepo.SaveChangesAsync();

            return new ApiResponse<object>(
                200,
                $"Order status changed from {oldStatus} to {newStatus}"
            );
        }

        public async Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersForAdminAsync(int page,
    int pageSize)
        {
            page = page <= 0 ? 1 : page;

            //int page = 1;
            //int pageSize = 10;
            pageSize = pageSize switch
            {
                <= 0 => 10,
                > 100 => 100,
                _ => pageSize
            };

            //The _ is a discard pattern in C# switch expressions. It means "everything else" or "default case".
            //If none of the above conditions match, use the original pageSize value

            var query = _orderRepo.Query();

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResult<OrderDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = orders.Select(Map).ToList()
            };

            return new ApiResponse<PagedResult<OrderDto>>(
                200,
                "All orders fetched successfully",
                response
            );
        }



    }
}
