using Dominus.Application.DTOs.OrderDTOs;
using Dominus.Application.DTOs.Payment;
using Dominus.Domain.Common;
using Dominus.Application.DTOs.OrderDTOs;
using Dominus.Domain.Enums;

namespace Dominus.Application.Interfaces.IServices
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto dto);
        Task<ApiResponse<List<OrderDto>>> GetMyOrdersAsync(string userId);
        Task<ApiResponse<List<OrderDto>>> GetOrdersByProductAsync(int productId,string userId);
        Task<ApiResponse<object>> AdminUpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<ApiResponse<PagedResult<OrderDto>>> GetAllOrdersForAdminAsync(int page,int pageSize, OrderStatus? status)
            ;

        Task<ApiResponse<object>> VerifyPaymentAsync(string userId, RazorVerifyDto dto);

        Task<ApiResponse<object>> CancelOrderAsync(string userId, int orderId);

    }
}
