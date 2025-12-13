using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Dominus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Infrastructure.Persistence
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                //.Include(p => p.Category)
                .Include(p => p.AvailableSizes)
                .Include(p => p.Images)
                .Where(p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            return await _context.Products
                //.Include(p => p.Category)
                .Include(p => p.AvailableSizes)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
