using Dominus.Application.DTOs.P_ImageDTOs;
using Dominus.Application.DTOs.ProductDTOs;
using Dominus.Application.Interfaces.IServices;
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
        [HttpGet("GetAll")]
        [AllowAnonymous]

        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            if (!products.Any())
            {
                return Ok(new ApiResponse<IEnumerable<ProductDto>>(
                    404,
                    "No products available",
                    Enumerable.Empty<ProductDto>()
                ));
            }
            return Ok(new ApiResponse<object>(200, "Products fetched successfully", products));
        }
        [HttpPost("Admin/Create")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid product data"));

            var result = await _productService.AddProductAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPatch("Admin/Toggle{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> ToggleStatus([Range(1, int.MaxValue)] int id)
        {
            var result = await _productService.ToggleProductStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        //[HttpPatch("{id}")]
        //[Authorize(Policy = "Admin")]
        //public async Task<IActionResult> PatchProduct(int id, [FromBody] PatchProductDto dto)
        //{
        //    var response = await _productService.PatchProductAsync(id, dto);
        //    return StatusCode(response.StatusCode, response);
        //}

       
        [HttpDelete("Admin/Delete{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete([Range(1, int.MaxValue)] int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("Admin/Update")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Update([FromForm] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(400, "Invalid product data"));

            var result = await _productService.UpdateProductAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        


        [HttpGet("GetCatBy_{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);

            if (!products.Any())
                return NotFound(new ApiResponse<object>(404, "No products found"));

            return Ok(new ApiResponse<object>(200, "Products fetched successfully", products));
        }

        [HttpGet("GetBy_{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound(new ApiResponse<object>(404, "Product not found"));

            return Ok(new ApiResponse<object>(200, "Product fetched successfully", product));
        }


        [HttpGet("Paged")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPaged(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetPagedProductsAsync(page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        //[HttpGet("filter")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetFilteredProducts(
        //  [FromQuery] string? name,
        //  [FromQuery] int? categoryId,
        //  [FromQuery] decimal? minPrice,
        //  [FromQuery] decimal? maxPrice,
        //  [FromQuery] bool? inStock,
        //  [FromQuery] int page = 1,
        //  [FromQuery] int pageSize = 20,
        //  [FromQuery] string? sortBy = null,
        //  [FromQuery] bool descending = false)
        //{
        //    var result = await _productService.GetFilteredProducts(
        //        name, categoryId, minPrice, maxPrice, inStock, page, pageSize, sortBy, descending);

        //    return StatusCode(result.StatusCode, result);
        //}

        [HttpGet("Filter_Sort")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterProducts([FromQuery] ProductFilterDto filter)
        {
            var result = await _productService.GetFilteredProductsAsync(filter);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("Admin/Add_Stock")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AddStock(
    [FromBody] AddProductStockDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>(
                    400,
                    "Invalid stock request"
                ));

            var result = await _productService.AddStockAsync(dto);
            return StatusCode(result.StatusCode, result);
        }



        [HttpGet("Search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(
    [FromQuery] string search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var result = await _productService
                .SearchProductsByNameAsync(search, page, pageSize);

            return StatusCode(result.StatusCode, result);
        }


        [HttpPost("Admin/AddImages")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> AddImages([FromForm] AddProductImagesDto dto)
        {
            var result = await _productService.AddProductImagesAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        //[HttpDelete("Admin/DeleteImage/{imageId}")]
        //[Authorize(Policy = "Admin")]
        //public async Task<IActionResult> DeleteImage(int imageId)
        //{
        //    var result = await _productService.DeleteProductImageAsync(imageId);
        //    return StatusCode(result.StatusCode, result);
        //}

    }
}
