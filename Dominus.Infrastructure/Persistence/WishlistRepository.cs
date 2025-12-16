using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Dominus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dominus.Infrastructure.Persistence
{
    public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
    {
        private readonly AppDbContext _context;

        public WishlistRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Wishlist?> GetByUserIdAsync(string userId)
        {
            return await _context.Wishlists
                .Include(w => w.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }
    }
}
