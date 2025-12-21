using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Dominus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dominus.Infrastructure.Persistence
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly AppDbContext _context;
        public CategoryRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Set<Category>()
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<Category?> GetWithProductsAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllActiveAsync()
        {
            return await _context.Categories
                        .Include(c => c.Products) 
                .Where(c => c.IsActive && !c.IsDeleted)
                .ToListAsync();
        }
    }
}