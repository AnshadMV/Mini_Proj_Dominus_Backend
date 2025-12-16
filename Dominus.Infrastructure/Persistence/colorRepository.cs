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
    public class ColorRepository : GenericRepository<Color>, IColorRepository
    {
        private readonly AppDbContext _context;

        public ColorRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Color?> GetByNameAsync(string name)
        {
            return await _context.Colors
                .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted);
        }
        public async Task<List<Color>> GetByIdsAsync(List<int> ids)
        {
            return await _context.Colors
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }
    }

}
