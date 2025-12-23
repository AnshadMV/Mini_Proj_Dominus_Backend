using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CartDTOs;

namespace Dominus.Application.Interfaces.IServices
{
    public interface ICartService
    {
        Task<ApiResponse<CartDto>> GetCartByUserAsync(string userId);
        Task<ApiResponse<CartDto>> AddToCartAsync(string userId, AddToCartDto dto);
        Task<ApiResponse<CartDto>> UpdateItemAsync(string userId, UpdateCartItemDto dto);
        Task<ApiResponse<bool>> RemoveItemAsync(string userId, int cartItemId);
        Task<ApiResponse<bool>> ClearCartAsync(string userId);
    }
}
