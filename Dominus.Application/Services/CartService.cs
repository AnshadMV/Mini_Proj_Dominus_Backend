using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CartDTOs;
using Dominus.Domain.Entities;

namespace Dominus.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly IGenericRepository<User> _userRepo;


        public CartService(
            ICartRepository cartRepo,
            IProductRepository productRepo,
            IGenericRepository<User> userRepo)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _userRepo = userRepo;
        }

        public async Task<ApiResponse<CartDto>> GetCartByUserAsync(string userId)
        {
            // 1️⃣ UserId validation
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<CartDto>(
                    401,
                    "User is not authenticated"
                );
            }

            // 2️⃣ Fetch user via GENERIC repository
            var user = await _userRepo.GetAsync(u => u.Id.ToString() == userId);

            if (user == null || user.IsDeleted)
            {
                return new ApiResponse<CartDto>(
                    404,
                    "User not found"
                );
            }

            if (user.IsBlocked)
            {
                return new ApiResponse<CartDto>(
                    403,
                    "User is blocked. Access denied"
                );
            }

            // 3️⃣ Fetch cart
            var cart = await _cartRepo.GetByUserIdAsync(userId);

            if (cart == null)
            {
                return new ApiResponse<CartDto>(
                    200,
                    "Cart is empty",
                    new CartDto { UserId = userId }
                );
            }

            // 4️⃣ No items
            if (cart == null || !cart.Items.Any())
            {
                return new ApiResponse<CartDto>(
                    200,
                    "No items in cart",
                    new CartDto
                    {
                        CartId = cart.Id,
                        UserId = userId
                    }
                );
            }

            var validItems = new List<CartItemDto>();
            var warnings = new List<string>();

            // 5️⃣ Item-level validation
            foreach (var item in cart.Items)
            {
                var product = item.Product;

                if (product == null || product.IsDeleted || !product.IsActive)
                {
                    warnings.Add($"Product removed (ID: {item.ProductId})");
                    continue;
                }

                if (!product.InStock || product.CurrentStock <= 0)
                {
                    warnings.Add($"{product.Name} is out of stock");
                    continue;
                }

                if (item.Quantity > product.CurrentStock)
                {
                    warnings.Add(
                        $"{product.Name} quantity adjusted to available stock ({product.CurrentStock})"
                    );
                    item.Quantity = product.CurrentStock;
                }

                validItems.Add(new CartItemDto
                {
                    Id = item.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = item.Quantity
                });
            }

            // 6️⃣ All items invalid
            if (!validItems.Any())
            {
                return new ApiResponse<CartDto>(
                    200,
                    "All cart items are unavailable",
                    new CartDto
                    {
                        CartId = cart.Id,
                        UserId = userId
                    }
                );
            }

            // 7️⃣ Final response
            var result = new CartDto
            {
                CartId = cart.Id,
                UserId = userId,
                Items = validItems
            };

            return new ApiResponse<CartDto>(
                200,
                warnings.Any()
                    ? "Cart fetched with warning count of cart's product count"
                    : "Cart fetched successfully",
                result
            );
        }


        public async Task<ApiResponse<CartDto>> AddToCartAsync(string userId, AddToCartDto dto)
        {
            // 1️⃣ UserId validation
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<CartDto>(401, "User is not authenticated");
            }

            // 2️⃣ Product existence
            var product = await _productRepo.GetByIdAsync(dto.ProductId);

            if (product == null || product.IsDeleted 
                //|| !product.IsActive
                )
            {
                return new ApiResponse<CartDto>(
                    404,
                    "Product not available"
                );
            }

            // 3️⃣ Stock validation
            if (!product.InStock || product.CurrentStock <= 0)
            {
                return new ApiResponse<CartDto>(
                    400,
                    "Product is out of stock"
                );
            }

            if (dto.Quantity > product.CurrentStock)
            {
                return new ApiResponse<CartDto>(
                    400,
                    $"Only {product.CurrentStock} items available"
                );
            }

            // 4️⃣ Get or create cart
            var cart = await _cartRepo.GetByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                await _cartRepo.AddAsync(cart);
                await _cartRepo.SaveChangesAsync(); // MUST save to get CartId
            }

            // 5️⃣ Add or update item
            var existingItem = cart.Items
                .FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem == null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }
            else
            {
                var newQty = existingItem.Quantity + dto.Quantity;

                if (newQty > product.CurrentStock)
                {
                    return new ApiResponse<CartDto>(
                        400,
                        $"Total quantity exceeds stock ({product.CurrentStock})"
                    );
                }

                existingItem.Quantity = newQty;
            }

            // 6️⃣ Save
            await _cartRepo.SaveChangesAsync();

            // 7️⃣ Reload cart (with products)
            var updatedCart = await _cartRepo.GetByUserIdAsync(userId);

            return new ApiResponse<CartDto>(
                200,
                "Item added to cart",
                Map(updatedCart!)
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
                return new ApiResponse<bool>(404, "Cart not found", false);

            var item = cart.Items.FirstOrDefault(i =>
                i.Id == cartItemId && !i.IsDeleted);

            if (item == null)
                return new ApiResponse<bool>(404, "Cart item not found", false);

            item.IsDeleted = true;

            await _cartRepo.SaveChangesAsync();

            return new ApiResponse<bool>(200, "Item removed from cart", true);
        }


        public async Task<ApiResponse<bool>> ClearCartAsync(string userId)
        {
            var cart = await _cartRepo.GetByUserIdAsync(userId);

            if (cart == null)
                return new ApiResponse<bool>(404, "Cart not found", false);

            var activeItems = cart.Items.Where(i => !i.IsDeleted).ToList();

            if (!activeItems.Any())
                return new ApiResponse<bool>(200, "Cart already empty", true);

            foreach (var item in activeItems)
            {
                item.IsDeleted = true;
            }

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
