using Dominus.Application.Services;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ProductDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dominus.WebAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {

        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateProductDTO dto)
        {
            try
            {
                // Log incoming request
                Console.WriteLine($"Received product creation request:");
                Console.WriteLine($"Name: {dto.Name}");
                Console.WriteLine($"Images count: {dto.Images?.Count}");

                if (dto == null)
                {
                    return BadRequest(new ApiResponse<object>(400, "Invalid request data"));
                }

                if (dto.Images == null || !dto.Images.Any())
                {
                    return BadRequest(new ApiResponse<object>(400, "At least one image is required"));
                }

                var product = await _productService.AddProductAsync(dto);
                return StatusCode(product.StatusCode, product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new ApiResponse<object>(500, $"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Update([FromForm] UpdateDto dto)
        {
            var product = await _productService.UpdateProductAsync(dto);
            return StatusCode(product.StatusCode, product);
        }

        [HttpPatch("status/{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ToggleStatus([Range(1, int.MaxValue)] int id)
        {
            var newStatus = await _productService.ToggleProductStatusAsync(id);
            return StatusCode(newStatus.StatusCode, newStatus);
        }

        [HttpGet("ByCategory/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            if (products == null || !products.Any())
                return NotFound(new ApiResponse<List<ProductDto>>(404, "No products found in this category"));

            return Ok(new ApiResponse<List<ProductDto>>(200, "Products fetched successfully", products.ToList()));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new ApiResponse<ProductDto>(404, "Product not found"));

            return Ok(new ApiResponse<ProductDto>(200, "Product fetched successfully", product));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new ApiResponse<List<ProductDto>>(200, "All products fetched successfully", products.ToList()));
        }


        [HttpGet("filter")]
        [AllowAnonymous]

        public async Task<IActionResult> GetFilteredProducts(
        [FromQuery] string? name,
        [FromQuery] int? categoryId,
        [FromQuery] string? brand,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? inStock,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = false)
        {
            var result = await _productService.GetFilteredProducts(
                name, categoryId, brand, minPrice, maxPrice, inStock, page, pageSize, sortBy, descending);

            return StatusCode(result.StatusCode, result);
        }

    }

}


