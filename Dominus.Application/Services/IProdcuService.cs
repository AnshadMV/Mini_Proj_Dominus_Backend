using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ProductDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.Services
{
    public interface IProductService
    {
        Task<ApiResponse<ProductDto>> AddProductAsync(CreateProductDTO dto);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateDto dto);
        Task<ApiResponse<string>> ToggleProductStatusAsync(int id);

        Task<ApiResponse<IEnumerable<ProductDto>>> GetFilteredProducts(
            string? name = null,
            int? categoryId = null,
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? inStock = null,
            int page = 1,
            int pageSize = 20,
            string? sortBy = null,
            bool descending = false
        );
    }
}
