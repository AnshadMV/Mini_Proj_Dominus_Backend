using Dominus.Application.Services;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ProductDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new ApiResponse<object>(200, "Products fetched successfully", products));
        }


        [HttpGet("ByCategory/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);

            if (!products.Any())
                return NotFound(new ApiResponse<object>(404, "No products found"));

            return Ok(new ApiResponse<object>(200, "Products fetched successfully", products));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound(new ApiResponse<object>(404, "Product not found"));

            return Ok(new ApiResponse<object>(200, "Product fetched successfully", product));
        }


        [HttpGet("paged")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaged(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetPagedProductsAsync(page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFilteredProducts(
          [FromQuery] string? name,
          [FromQuery] int? categoryId,
          [FromQuery] decimal? minPrice,
          [FromQuery] decimal? maxPrice,
          [FromQuery] bool? inStock,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20,
          [FromQuery] string? sortBy = null,
          [FromQuery] bool descending = false)
        {
            var result = await _productService.GetFilteredProducts(
                name, categoryId, minPrice, maxPrice, inStock, page, pageSize, sortBy, descending);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid product data"));

            var result = await _productService.AddProductAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Update([FromForm] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid product data"));

            var result = await _productService.UpdateProductAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
       
        [HttpPatch("status/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ToggleStatus([Range(1, int.MaxValue)] int id)
        {
            var result = await _productService.ToggleProductStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return StatusCode(result.StatusCode, result);
        }

    }
}
