using Dominus.Domain.Common;
using Dominus.Domain.DTOs.OrderDTOs;

namespace Dominus.Application.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<ApiResponse<List<OrderDto>>> GetMyOrdersAsync(string userId);
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(string userId, int orderId);
    }
}
