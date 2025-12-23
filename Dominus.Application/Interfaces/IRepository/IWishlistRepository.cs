using Dominus.Domain.Entities;

namespace Dominus.Application.Interfaces.IRepository
{
    public interface IWishlistRepository : IGenericRepository<Wishlist>
    {
        Task<Wishlist?> GetByUserIdAsync(string userId);
    }
}
