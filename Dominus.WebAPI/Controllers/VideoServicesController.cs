using Dominus.Application.DTOs.VideoServices;
using Dominus.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoServicesController : ControllerBase
    {
        private readonly IVideoService _service;

        public VideoServicesController(IVideoService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int? limit)
        {
            var result = await _service.GetFeaturedAsync(limit);
            return Ok(result);
        }


        [HttpPost]
        [Authorize(Policy = "admin")]
        public async Task<IActionResult> Add(CreateVideoServiceDto dto)
        {
            await _service.AddAsync(dto);
            return Ok(new { message = "Video service added successfully" });
        }
    }
}
