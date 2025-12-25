using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Dominus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dominus.Infrastructure.Persistence
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                 .Include(o => o.Items)
        .ThenInclude(i => i.Color)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }




        public async Task<Order?> GetByIdWithItemsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                 .Include(o => o.Items)
        .ThenInclude(i => i.Color)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetByUserAndProductAsync(
    string userId,
    int productId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                    .Include(o => o.Items)
        .ThenInclude(i => i.Color)
                .Where(o =>
                    o.UserId == userId &&
                    o.Items.Any(i => i.ProductId == productId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }







        public async Task<List<Order>> GetAllWithItemsAsync()
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }








        public IQueryable<Order>  Query()
        {
            return _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                     .Include(o => o.Items)
            .ThenInclude(i => i.Color)
                .OrderByDescending(o => o.OrderDate);
        }


    }
}
