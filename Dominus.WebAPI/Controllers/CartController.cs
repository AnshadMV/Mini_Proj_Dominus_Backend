using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.DTOs.CartDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "user")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
       public CartController(ICartService service)
        {
            _service = service;
        }

        private string UserId => User.FindFirstValue("userId")!;

        [HttpGet("myCart")]
        public async Task<IActionResult> GetCart()
            => Ok(await _service.GetCartByUserAsync(UserId));

        [HttpPost("add")]
        public async Task<IActionResult> Add(AddToCartDto dto)
        {
            var response = await _service.AddToCartAsync(UserId, dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update")]

        public async Task<IActionResult> Update(UpdateCartItemDto dto)
            => Ok(await _service.UpdateItemAsync(UserId, dto));

        [HttpDelete("delete/{id}")]

        public async Task<IActionResult> Remove(int id)
            => Ok(await _service.RemoveItemAsync(UserId, id));

        [HttpDelete("clear")]
        public async Task<IActionResult> Clear()
            => Ok(await _service.ClearCartAsync(UserId));
    }
}
