using Dominus.Domain.Entities;
using Dominus.Domain.Entities.VideoServices;

namespace Dominus.Application.Interfaces
{
    public interface IVideoServiceRepository
    {
        Task AddAsync(VideoService entity);

        Task<List<VideoService>> GetAllAsync();
        Task<List<VideoService>> GetFeaturedAsync(int? limit);
    }
}
