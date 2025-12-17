using Dominus.Domain.Common;
using Dominus.Domain.DTOs.CategoryDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dominus.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _genericRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IProductRepository _productRepo;

        public CategoryService(
            IGenericRepository<Category> genericRepo,
            ICategoryRepository categoryRepo,
            IProductRepository productRepo)
        {
            _genericRepo = genericRepo;
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
        }

        public async Task<ApiResponse<CategoryDto>> AddCategoryAsync(CreateCategoryDto dto)
        {
            if (dto == null) return new ApiResponse<CategoryDto>(400, "Invalid category data");

            var exists = await _categoryRepo.GetByNameAsync(dto.Name.Trim());
            if (exists != null)
                return new ApiResponse<CategoryDto>(409, "Category with same name already exists");

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = dto.IsActive
            };

            await _genericRepo.AddAsync(category);
            await _genericRepo.SaveChangesAsync();

            return new ApiResponse<CategoryDto>(201, "Category created", MapToDto(category));
        }

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
            if (dto == null) return new ApiResponse<CategoryDto>(400, "Invalid category data");

            var category = await _genericRepo.GetByIdAsync(dto.Id);
            if (category == null || category.IsDeleted)
                return new ApiResponse<CategoryDto>(404, "Category not found");

            var otherWithSameName = await _categoryRepo.GetByNameAsync(dto.Name.Trim());
            if (otherWithSameName != null && otherWithSameName.Id != dto.Id)
                return new ApiResponse<CategoryDto>(409, "Another category with same name exists");

            category.Name = dto.Name.Trim();
            category.Description = dto.Description?.Trim();
            category.IsActive = dto.IsActive;
            category.ModifiedOn = DateTime.UtcNow;

            _genericRepo.Update(category);
            await _genericRepo.SaveChangesAsync();

            return new ApiResponse<CategoryDto>(200, "Category updated", MapToDto(category));
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepo.GetAllActiveAsync();
            var result = categories.Select(c => MapToDto(c));
            return new ApiResponse<IEnumerable<CategoryDto>>(200, "Categories fetched", result);
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepo.GetWithProductsAsync(id);
            if (category == null || category.IsDeleted)
                return new ApiResponse<CategoryDto>(404, "Category not found");

            return new ApiResponse<CategoryDto>(200, "Category fetched", MapToDto(category));
        }

        public async Task<ApiResponse<string>> ToggleCategoryStatusAsync(int id)
        {
            var category = await _genericRepo.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
                return new ApiResponse<string>(404, "Category not found");

            category.IsActive = !category.IsActive;
            _genericRepo.Update(category);
            await _genericRepo.SaveChangesAsync();

            return new ApiResponse<string>(200, category.IsActive ? "Category activated" : "Category deactivated");
        }

        public async Task<ApiResponse<string>> SoftDeleteCategoryAsync(int id)
        {
            var category = await _categoryRepo.GetWithProductsAsync(id);
            if (category == null || category.IsDeleted)
                return new ApiResponse<string>(404, "Category not found");

            if (category.Products != null && category.Products.Any(p => !p.IsDeleted))
                return new ApiResponse<string>(400, "Cannot delete category with existing products");

            _genericRepo.Delete(category);
            await _genericRepo.SaveChangesAsync();

            return new ApiResponse<string>(200, "Category deleted", "deleted");
        }

        private static CategoryDto MapToDto(Category c)
        {
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products?.Count ?? 0
            };
        }
    }
}