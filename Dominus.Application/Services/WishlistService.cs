using Dominus.Application.Interfaces;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CartDTOs;
using Dominus.Domain.DTOs.WishlistDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;

namespace Dominus.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly IProductRepository _productRepo;

        public WishlistService(
            IWishlistRepository wishlistRepo,
            IProductRepository productRepo)
        {
            _wishlistRepo = wishlistRepo;
            _productRepo = productRepo;
        }

        public async Task<WishlistDto> GetWishlistAsync(string userId)
        {
            var wishlist = await _wishlistRepo.GetByUserIdAsync(userId);
            return wishlist == null
                ? new WishlistDto { UserId = userId }
                : Map(wishlist);
        }

        public async Task<ApiResponse<WishlistDto>> AddAsync(string userId, AddToWishlistDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);

            if (product == null || product.IsDeleted || !product.IsActive)
            {
                return new ApiResponse<WishlistDto>(
                    404,
                    "Product not found"
                );
            }
            //if (!product.InStock || product.CurrentStock < 0)
            //{
            //    return new ApiResponse<WishlistDto>(
            //        400,
            //        "Product out of stock"
            //    );
            //}

            var wishlist = await _wishlistRepo.GetByUserIdAsync(userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                await _wishlistRepo.AddAsync(wishlist);
                await _wishlistRepo.SaveChangesAsync();
            }

            var existingItem = wishlist.Items
                .FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                wishlist.Items.Remove(existingItem);
                await _wishlistRepo.SaveChangesAsync();

                var reloadedAfterRemove =
                    await _wishlistRepo.GetByUserIdAsync(userId);

                return new ApiResponse<WishlistDto>(
                    200,
                    "Product removed from wishlist",
                    reloadedAfterRemove == null
                        ? new WishlistDto { UserId = userId }
                        : Map(reloadedAfterRemove)
                );
            }

            wishlist.Items.Add(new WishlistItem
            {
                ProductId = dto.ProductId
            });

            await _wishlistRepo.SaveChangesAsync();

            var reloadedAfterAdd =
                await _wishlistRepo.GetByUserIdAsync(userId);

            return new ApiResponse<WishlistDto>(
                200,
                "Product added to wishlist",
                Map(reloadedAfterAdd!)
            );
        }


        public async Task<ApiResponse<bool>> RemoveAsync(string userId, int wishlistItemId)
        {
            var wishlist = await _wishlistRepo.GetByUserIdAsync(userId);

            if (wishlist == null)
                return new ApiResponse<bool>(404, "Wishlist not found", false);

            var item = wishlist.Items.FirstOrDefault(i => i.Id == wishlistItemId);

            if (item == null)
                return new ApiResponse<bool>(404, "Wishlist item not found", false);

            wishlist.Items.Remove(item);
            await _wishlistRepo.SaveChangesAsync();

            return new ApiResponse<bool>(200, "Item removed from wishlist", true);
        }

        public async Task<ApiResponse<bool>> ClearAsync(string userId)
        {
            var wishlist = await _wishlistRepo.GetByUserIdAsync(userId);

            if (wishlist == null)
                return new ApiResponse<bool>(404, "Wishlist not found", false);

            wishlist.Items.Clear();
            await _wishlistRepo.SaveChangesAsync();

            return new ApiResponse<bool>(200, "Wishlist cleared", true);
        }

        private static WishlistDto Map(Wishlist wishlist) => new()
        {
            WishlistId = wishlist.Id,
            UserId = wishlist.UserId,
            Items = wishlist.Items.Select(i => new WishlistItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                Price = i.Product?.Price ?? 0
            }).ToList()
        };
    }
}
