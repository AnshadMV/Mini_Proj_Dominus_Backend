using Dominus.Application.Interfaces;
using Dominus.Domain.Entities.VideoServices;
using Dominus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dominus.Infrastructure.Repositories
{
    public class VideoServiceRepository : IVideoServiceRepository
    {
        private readonly AppDbContext _context;

        public VideoServiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VideoService>> GetAllAsync()
        {
            return await _context.VideoServices
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<VideoService>> GetFeaturedAsync(int? limit)
        {
            var query = _context.VideoServices
                .Where(x => x.IsFeatured)
                .AsNoTracking();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }
        public async Task AddAsync(VideoService entity)
        {
            _context.VideoServices.Add(entity);
            await _context.SaveChangesAsync();
        }

    }
}
