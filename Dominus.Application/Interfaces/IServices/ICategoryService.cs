using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CategoryDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dominus.Application.Interfaces.IServices
{
    public interface ICategoryService
    {
        Task<ApiResponse<CreateCategoryDto>> AddCategoryAsync(CreateCategoryDto dto);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto dto);
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<string>> ToggleCategoryStatusAsync(int id);
        Task<ApiResponse<string>> SoftDeleteCategoryAsync(int id);
    }
}