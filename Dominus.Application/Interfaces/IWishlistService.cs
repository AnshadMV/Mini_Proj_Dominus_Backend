using Dominus.Domain.Common;
using Dominus.Domain.DTOs.WishlistDTOs;

namespace Dominus.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<WishlistDto> GetWishlistAsync(string userId);
        Task<ApiResponse<WishlistDto>> AddAsync(string userId, AddToWishlistDto dto);
        Task<ApiResponse<bool>> RemoveAsync(string userId, int wishlistItemId);
        Task<ApiResponse<bool>> ClearAsync(string userId);
    }
}
