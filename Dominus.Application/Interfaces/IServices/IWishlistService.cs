using Dominus.Domain.Common;
using Dominus.Application.DTOs.WishlistDTOs;

namespace Dominus.Application.Interfaces.IServices
{
    public interface IWishlistService
    {
        Task<ApiResponse<WishlistDto>> GetWishlistAsync(string userId);
        Task<ApiResponse<WishlistDto>> ToggleAsync(string userId, int productId);
        //Task<ApiResponse<bool>> RemoveAsync(string userId, int wishlistItemId);
        Task<ApiResponse<bool>> ClearAsync(string userId);
    }
}
