using Dominus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> Create()
        {
            var response = await _service.CreateOrderAsync(UserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("my")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> MyOrders()
        {
            var response = await _service.GetMyOrdersAsync(UserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _service.GetOrderByIdAsync(UserId, id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
