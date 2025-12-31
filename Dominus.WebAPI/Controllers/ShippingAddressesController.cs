using Dominus.Application.DTOs.ShippingAddress;
using Dominus.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dominus.WebAPI.Controllers
{
    [Route("api/shipping-address")]
    [ApiController]
    [Authorize]
    public class ShippingAddressesController : ControllerBase
    {
        private readonly IShippingAddressService _service;

        public ShippingAddressesController(IShippingAddressService service)
        {
            _service = service;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(ShippingAddressRequestDto dto)
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var response = await _service.AddAsync(userId, dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyAddresses()
        {
            var response = await _service.GetMyAddressesAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(
    int id,
    UpdateShippingAddressRequestDto dto)
        {
            var response = await _service.UpdateAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("set-active/{id}")]
        public async Task<IActionResult> SetActive(int id)
        {
            var response = await _service.SetActiveAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
