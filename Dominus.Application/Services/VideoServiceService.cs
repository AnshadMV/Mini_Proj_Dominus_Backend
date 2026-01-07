using Dominus.Application.DTOs;
using Dominus.Application.DTOs.VideoServices;
using Dominus.Application.Interfaces;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Entities.VideoServices;

namespace Dominus.Application.Services
{
    public class VideoServiceService : IVideoService
    {
        private readonly IVideoServiceRepository _repo;

        public VideoServiceService(IVideoServiceRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<VideoServiceDto>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();

            return data.Select(x => new VideoServiceDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                VideoUrl = x.VideoUrl,
                LearnMoreLink = x.LearnMoreLink,
                Thumbnail = x.Thumbnail
            }).ToList();
        }

        public async Task<List<VideoServiceDto>> GetFeaturedAsync(int? limit)
        {
            var data = await _repo.GetFeaturedAsync(limit);

            return data.Select(x => new VideoServiceDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                VideoUrl = x.VideoUrl,
                LearnMoreLink = x.LearnMoreLink,
                Thumbnail = x.Thumbnail
            }).ToList();
        }
        public async Task AddAsync(CreateVideoServiceDto dto)
        {
            var entity = new VideoService
            {
                Title = dto.Title,
                Description = dto.Description,
                VideoUrl = dto.VideoUrl,
                LearnMoreLink = dto.LearnMoreLink,
                Thumbnail = dto.Thumbnail,
                IsFeatured = dto.IsFeatured
            };

            await _repo.AddAsync(entity);
        }

    }
}
