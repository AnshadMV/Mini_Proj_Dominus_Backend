using Dominus.Application.Interfaces;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.OrderDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;

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

            var order = new Order
            {
                UserId = userId
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

                product.CurrentStock -= item.Quantity;
                product.InStock = product.CurrentStock > 0;

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

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(string userId, int orderId)
        {
            var order = await _orderRepo.GetByIdWithItemsAsync(orderId);

            if (order == null || order.UserId != userId)
                return new ApiResponse<OrderDto>(404, "Order not found");

            return new ApiResponse<OrderDto>(
                200,
                "Order fetched",
                Map(order)
            );
        }

        private static OrderDto Map(Order order) => new()
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };
    }
}
