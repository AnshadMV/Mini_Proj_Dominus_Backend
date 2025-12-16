using Dominus.Application.Interfaces;
using Dominus.Domain.DTOs.WishlistDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/wishlist")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _service;

        public WishlistController(IWishlistService service)
        {
            _service = service;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        [Authorize(Policy = "user")]

        public async Task<IActionResult> Get()
            => Ok(await _service.GetWishlistAsync(UserId));

        [HttpPost("add")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> Add(AddToWishlistDto dto)
        {
            var response = await _service.AddAsync(UserId, dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("item/{id}")]
        [Authorize(Policy = "user")]

        public async Task<IActionResult> Remove(int id)
            => Ok(await _service.RemoveAsync(UserId, id));

        [HttpDelete("clear")]
        [Authorize(Policy = "user")]

        public async Task<IActionResult> Clear()
            => Ok(await _service.ClearAsync(UserId));
    }
}
