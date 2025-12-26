using Dominus.Application.Interfaces.IRepository;
using Dominus.Domain.Entities;
using Dominus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dominus.Infrastructure.Persistence
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Images)

                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
