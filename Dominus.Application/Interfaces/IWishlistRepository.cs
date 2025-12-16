using Dominus.Domain.Entities;

namespace Dominus.Domain.Interfaces
{
    public interface IWishlistRepository : IGenericRepository<Wishlist>
    {
        Task<Wishlist?> GetByUserIdAsync(string userId);
    }
}
