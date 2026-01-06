using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Application.DTOs.CategoryDTOs;
using Dominus.Domain.Entities;


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

        public async Task<ApiResponse<CreateCategoryDto>> AddCategoryAsync(CreateCategoryDto dto)
        {
            if (dto == null) return new ApiResponse<CreateCategoryDto>(400, "Invalid category data");

            var exists = await _categoryRepo.GetByNameAsync(dto.Name.Trim());
            if (exists != null)
                return new ApiResponse<CreateCategoryDto>(409, "Category with same name already exists");

            var category = new Category
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = dto.IsActive
            };

            await _genericRepo.AddAsync(category);
            await _genericRepo.SaveChangesAsync();

            return new ApiResponse<CreateCategoryDto>(200, "Category created", new CreateCategoryDto
            {
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            });
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
            var category = await _categoryRepo.GetWithProductsAsync(id);
            if (category == null || category.IsDeleted)
                return new ApiResponse<string>(404, "Category not found");

            category.IsActive = !category.IsActive;

            if (category.Products != null)
            {
                foreach (var product in category.Products.Where(p => !p.IsDeleted))
                {
                    product.IsActive = category.IsActive;
                    product.ModifiedOn = DateTime.UtcNow;
                    product.ModifiedBy = "system";
                }
            }

            _genericRepo.Update(category);
            await _genericRepo.SaveChangesAsync();

            return new ApiResponse<string>(
                200,
                category.IsActive
                    ? "Category and products activated"
                    : "Category and products deactivated"
            );
        }


        public async Task<ApiResponse<string>> SoftDeleteCategoryAsync(int id)
        {
            var category = await _categoryRepo.GetWithProductsAsync(id);
            if (category == null || category.IsDeleted)
                return new ApiResponse<string>(404, "Category not found");

            //if (category.Products != null && category.Products.Any(p => !p.IsDeleted))
            //    return new ApiResponse<string>(400, "Cannot delete category with existing products");

            if (category.Products != null && category.Products.Any())
            {
                foreach (var product in category.Products.Where(p => !p.IsDeleted))
                {
                    //product.IsDeleted = true;
                    product.IsActive = false;
                    product.DeletedOn = DateTime.UtcNow;
                    product.DeletedBy = "system";
                }
            }

            category.IsDeleted = true;
            category.IsActive = false;

            category.DeletedOn = DateTime.UtcNow;
            category.DeletedBy = "system";

            _genericRepo.Delete(category);
            await _genericRepo.SaveChangesAsync();

            return new ApiResponse<string>(201, "Category deleted", "deleted");
        }

        private static CategoryDto MapToDto(Category c)
        {
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products?
            .Count(p => p.IsActive && !p.IsDeleted) ?? 0
            };
        }
    }
}