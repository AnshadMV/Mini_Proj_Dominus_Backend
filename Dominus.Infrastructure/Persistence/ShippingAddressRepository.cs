using Dominus.Application.Interfaces.IRepository;
using Dominus.Domain.Entities;
using Dominus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dominus.Infrastructure.Persistence
{
    public class ShippingAddressRepository : IShippingAddressRepository
    {
        private readonly AppDbContext _context;

        public ShippingAddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ShippingAddress address)
        {
            await _context.ShippingAddresses.AddAsync(address);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ShippingAddress>> GetByUserIdAsync(int userId)
        {
            return await _context.ShippingAddresses
                .Where(sa => sa.UserId == userId)
                .ToListAsync();
        }

        public async Task<ShippingAddress?> GetByIdAsync(int id)
        {
            return await _context.ShippingAddresses.FindAsync(id);
        }

        public async Task SoftDeleteAsync(ShippingAddress address)
        {
            address.IsDeleted = true;
            _context.ShippingAddresses.Update(address);
            await _context.SaveChangesAsync();
        }
    }
}
