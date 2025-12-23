using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CategoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet("Get_All")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("GetBy_{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (result.StatusCode != 200) return StatusCode(result.StatusCode, result);
            return Ok(result);
        }


        [HttpPost("Admin/Create")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid category data"));

            var result = await _categoryService.AddCategoryAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("Admin/Update")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid category data"));

            var result = await _categoryService.UpdateCategoryAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("Admin/toggle/status")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        {
            var result = await _categoryService.ToggleCategoryStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("Admin/DeleteBy_{id}")]

        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.SoftDeleteCategoryAsync(id);
            return StatusCode(result.StatusCode, result);
        }

       

       
    }
}