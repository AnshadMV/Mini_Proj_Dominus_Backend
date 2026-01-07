using Dominus.Application.DTOs;
using Dominus.Application.DTOs.VideoServices;

namespace Dominus.Application.Interfaces.IServices
{
    public interface IVideoService
    {
        Task AddAsync(CreateVideoServiceDto dto);

        Task<List<VideoServiceDto>> GetAllAsync();
        Task<List<VideoServiceDto>> GetFeaturedAsync(int? limit);
    }
}
