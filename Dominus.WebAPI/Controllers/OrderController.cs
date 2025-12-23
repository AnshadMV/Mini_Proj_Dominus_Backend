using Dominus.Application.DTOs.Payment;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.DTOs.OrderDTOs;
using Dominus.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]

    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        private string UserId => User.FindFirstValue("userId")!;
        [HttpPost("Add")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var response = await _service.CreateOrderAsync(UserId, dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("MyOrders")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> MyOrders()
        {
            var response = await _service.GetMyOrdersAsync(UserId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("GetBy_{Id}")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> GetOrdersByProduct(int productId)
        {
            var userId = User.FindFirstValue("userId")!;
            var response = await _service.GetOrdersByProductAsync(productId, userId);
            return StatusCode(response.StatusCode, response);
        }



        [HttpPost("PayBy_{orderId}")]
        [Authorize(Policy = "user")]
        public async Task<IActionResult> PayForOrder(int orderId, PaymentDto dto)
        {
            var response = await _service.PayForOrderAsync(UserId, orderId, dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("Admin/toggle/OrderStatus")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ChangeStatus(
    int orderId,
    [FromQuery] OrderStatus status)
        {
            var response = await _service.AdminUpdateOrderStatusAsync(orderId, status);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("Admin/GetAllOrders")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAllOrdersForAdmin(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var response = await _service.GetAllOrdersForAdminAsync(page, pageSize);
            return StatusCode(response.StatusCode, response);
        }





    }
}
