using Dominus.Application.Interfaces;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CartDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;

namespace Dominus.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;

        public CartService(ICartRepository cartRepo, IProductRepository productRepo)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
        }

        public async Task<CartDto> GetCartByUserAsync(string userId)
        {
            var cart = await _cartRepo.GetByUserIdAsync(userId);
            return cart == null
                ? new CartDto { UserId = userId }
                : Map(cart);
        }

        public async Task<ApiResponse<CartDto>> AddToCartAsync(string userId, AddToCartDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);

            if (!product.InStock || product.CurrentStock < dto.Quantity)
            {
                return new ApiResponse<CartDto>(
                    400,
                    "Product out of stock"
                );
            }
            if (product == null || product.IsDeleted || !product.IsActive)
            {
                return new ApiResponse<CartDto>(
                    404,
                    "Product not available"
                );
            }


            var cart = await _cartRepo.GetByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepo.AddAsync(cart);
                await _cartRepo.SaveChangesAsync();
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }
            else
            {
                item.Quantity += dto.Quantity;
            }

            await _cartRepo.SaveChangesAsync();

            var reloadedCart = await _cartRepo.GetByUserIdAsync(userId);

            return new ApiResponse<CartDto>(
                200,
                "Item added to cart",
                Map(reloadedCart!)
            );
        }

        public async Task<ApiResponse<CartDto>> UpdateItemAsync(string userId, UpdateCartItemDto dto)
        {
            var cart = await _cartRepo.GetByUserIdAsync(userId);

            if (cart == null)
            {
                return new ApiResponse<CartDto>(
                    404,
                    "Cart not found"
                );
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == dto.CartItemId);

            if (item == null)
            {
                return new ApiResponse<CartDto>(
                    404,
                    "Cart item not found"
                );
            }

            item.Quantity = dto.Quantity;
            await _cartRepo.SaveChangesAsync();

            return new ApiResponse<CartDto>(
                200,
                "Cart item updated",
                Map(cart)
            );
        }

        public async Task<ApiResponse<bool>> RemoveItemAsync(string userId, int cartItemId)
        {
            var cart = await _cartRepo.GetByUserIdAsync(userId);

            if (cart == null)
            {
                return new ApiResponse<bool>(404, "Cart not found", false);
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item == null)
            {
                return new ApiResponse<bool>(404, "Cart item not found", false);
            }

            cart.Items.Remove(item);
            await _cartRepo.SaveChangesAsync();

            return new ApiResponse<bool>(200, "Item removed from cart", true);
        }

        public async Task<ApiResponse<bool>> ClearCartAsync(string userId)
        {
            var cart = await _cartRepo.GetByUserIdAsync(userId);

            if (cart == null)
            {
                return new ApiResponse<bool>(404, "Cart not found", false);
            }

            cart.Items.Clear();
            await _cartRepo.SaveChangesAsync();

            return new ApiResponse<bool>(200, "Cart cleared", true);
        }

        private static CartDto Map(Cart cart) => new()
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                Price = i.Product?.Price ?? 0,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
