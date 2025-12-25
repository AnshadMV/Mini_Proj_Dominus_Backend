using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.DTOs.WishlistDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _service;

        public WishlistController(IWishlistService service)
        {
            _service = service;
        }

        private string UserId => User.FindFirstValue("userId")!;

        
     

        [HttpGet("MyWishlist")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> Get()
        {
            var response = await _service.GetWishlistAsync(UserId);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost("Toggle/{productId:int}")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> Toggle(int productId)
        {
            var response = await _service.ToggleAsync(UserId, productId);
            return StatusCode(response.StatusCode, response);
        }


        //[HttpDelete("item/{id}")]
        //[Authorize(Policy = "user")]

        //public async Task<IActionResult> Remove(int id)
        //    => Ok(await _service.RemoveAsync(UserId, id));

        [HttpDelete("Clear")]
        [Authorize(Policy = "user")]

        public async Task<IActionResult> Clear()
            => Ok(await _service.ClearAsync(UserId));
    }
}
