using Dominus.Application.DTOs.P_ImageDTOs;
using Dominus.Application.DTOs.ProductDTOs;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ProductDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.Interfaces.IServices
{
    public interface IProductService
    {
        Task<ApiResponse<ProductDto>> AddProductAsync(CreateProductDto dto);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto dto);

        Task<ApiResponse<string>> DeleteProductAsync(int id);
        Task<ApiResponse<ProductDto>> PatchProductAsync(int id, PatchProductDto dto);
        Task<ApiResponse<ProductDto>> AddStockAsync(AddProductStockDto dto);

        Task<ApiResponse<string>> ToggleProductStatusAsync(int id);
        Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(int page = 1, int pageSize = 10);
        Task<ApiResponse<PagedResult<ProductDto>>>  GetFilteredProductsAsync(ProductFilterDto filter);
        Task<ApiResponse<PagedResult<ProductDto>>>SearchProductsByNameAsync(string search, int page = 1, int pageSize = 10);
        Task<ApiResponse<List<string>>> AddProductImagesAsync(AddProductImagesDto dto);
        Task<ApiResponse<bool>> DeleteProductImageAsync(int imageId);


    }
}
