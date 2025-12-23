using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CartDTOs;
using Dominus.Domain.DTOs.WishlistDTOs;
using Dominus.Domain.Entities;

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

        public async Task<ApiResponse<WishlistDto>> GetWishlistAsync(string userId)
        {

            // 1️⃣ UserId validation
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<WishlistDto>(
                    401,
                    "User is not authenticated"
                );
            }

            // 2️⃣ Fetch wishlist
            var wishlist = await _wishlistRepo.GetByUserIdAsync(userId);

            if (wishlist == null)
            {
                return new ApiResponse<WishlistDto>(
                    200,
                    "Wishlist is empty",
                    new WishlistDto { UserId = userId }
                );
            }

            // 3️⃣ No items
            if (!wishlist.Items.Any())
            {
                return new ApiResponse<WishlistDto>(
                    200,
                    "No items in wishlist",
                    new WishlistDto
                    {
                        WishlistId = wishlist.Id,
                        UserId = userId
                    }
                );
            }

            var validItems = new List<WishlistItemDto>();
            var warnings = new List<string>();

            // 4️⃣ Product-level validation
            foreach (var item in wishlist.Items)
            {
                var product = item.Product;

                if (product == null || product.IsDeleted || !product.IsActive)
                {
                    warnings.Add($"Product removed (ID: {item.ProductId})");
                    continue;
                }

                validItems.Add(new WishlistItemDto
                {
                    Id = item.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price
                });
            }

            // 5️⃣ All items invalid
            if (!validItems.Any())
            {
                return new ApiResponse<WishlistDto>(
                    200,
                    "All wishlist items are unavailable",
                    new WishlistDto
                    {
                        WishlistId = wishlist.Id,
                        UserId = userId
                    }
                );
            }

            // 6️⃣ Final response
            return new ApiResponse<WishlistDto>(
                200,
                warnings.Any()
                    ? "Wishlist fetched with warnings"
                    : "Wishlist fetched successfully",
                new WishlistDto
                {
                    WishlistId = wishlist.Id,
                    UserId = userId,
                    Items = validItems
                }
            );
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
            if (wishlist != null && wishlist.IsDeleted)
            {
                return new ApiResponse<WishlistDto>(
                    403,
                    "Wishlist is deleted and cannot be modified"
                );
            }
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


        //public async Task<ApiResponse<bool>> RemoveAsync(string userId, int wishlistItemId)
        //{
        //    var wishlist = await _wishlistRepo.GetByUserIdAsync(userId);

        //    if (wishlist == null)
        //        return new ApiResponse<bool>(404, "Wishlist not found", false);

        //    var item = wishlist.Items.FirstOrDefault(i => i.Id == wishlistItemId);

        //    if (item == null)
        //        return new ApiResponse<bool>(404, "Wishlist item not found", false);

        //    wishlist.Items.Remove(item);
        //    await _wishlistRepo.SaveChangesAsync();

        //    return new ApiResponse<bool>(200, "Item removed from wishlist", true);
        //}

        public async Task<ApiResponse<bool>> ClearAsync(string userId)
        {
            // 1️⃣ Fetch wishlist
            var wishlist = await _wishlistRepo.GetByUserIdAsync(userId);

            if (wishlist == null)
                return new ApiResponse<bool>(404, "Wishlist not found", false);

            // 2️⃣ Get active (non-deleted) items
            var activeItems = wishlist.Items
                .Where(i => !i.IsDeleted)
                .ToList();

            if (!activeItems.Any())
                return new ApiResponse<bool>(200, "Wishlist already empty", true);

            // 3️⃣ Soft delete items
            foreach (var item in activeItems)
            {
                item.IsDeleted = true;
            }

            // 4️⃣ Save
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
