using Dominus.Application.Services;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ColorDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Dominus.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/colors")]
    
    public class ColorsController : ControllerBase
    {
        private readonly IColorService _colorService;

        public ColorsController( IColorService colorService)
        {
            _colorService = colorService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var response = await _colorService.GetAllAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateColorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid color data"));

            var response = await _colorService.CreateAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Authorize(Policy= "Admin")]
        public async Task<IActionResult> Update([FromBody]  UpdateColorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid color data"));

            var result = await _colorService.UpdateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("status/{id:int}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponse<object>(400, "Invalid color id"));

            var result = await _colorService.ToggleStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }



        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteColors(int id)
        {
            var result = await _colorService.SoftDeleteColorAsync(id);
            return StatusCode(result.StatusCode, result);
        }


    }


}
