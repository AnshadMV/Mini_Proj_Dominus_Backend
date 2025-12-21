using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ProductDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dominus.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly IProductRepository _productRepository;
        private readonly IColorRepository _colorRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(
            IGenericRepository<Product> repository,
            IProductRepository productRepository,
            IColorRepository colorRepository,
           ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
            _colorRepository = colorRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<ApiResponse<ProductDto>> AddProductAsync(CreateProductDto dto)
        {
            var normalizedName = dto.Name.Trim().ToLower();

            var existingProduct = await _repository.GetAsync(
                p => !p.IsDeleted &&
                     p.Name.ToLower() == normalizedName.ToLower()
            );

            if (existingProduct != null)
            {
                return new ApiResponse<ProductDto>(
                    409,
                    $"Product '{normalizedName}' already exists"
                );
            }

            if (dto == null)
                return new ApiResponse<ProductDto>(400, "Invalid product data");

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null || category.IsDeleted)
                return new ApiResponse<ProductDto>(404, "Category not found");

            if (!category.IsActive)
                return new ApiResponse<ProductDto>(400, "Category is inactive");

            if (!dto.InStock && dto.CurrentStock > 0)
                return new ApiResponse<ProductDto>(400, "Stock conflicting");


            var colors = new List<Color>();

            if (dto.ColorIds != null && dto.ColorIds.Any())
            {

                if (dto.ColorIds.Count != dto.ColorIds.Distinct().Count())
                    return new ApiResponse<ProductDto>(
                        400,
                        "Duplicate colors are not allowed"
                    );


                colors = await _colorRepository.GetByIdsAsync(dto.ColorIds);

                if (colors.Count != dto.ColorIds.Count)
                    return new ApiResponse<ProductDto>(400, "One or more colors not found");

                if (colors.Any(c => !c.IsActive))
                    return new ApiResponse<ProductDto>(400, "One or more colors are inactive");
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                CurrentStock = dto.CurrentStock,
                InStock = dto.CurrentStock > 0,
                IsActive = dto.IsActive,
                TopSelling = dto.TopSelling,
                Status = dto.Status,
                Warranty = dto.Warranty
            };

            if (dto.ColorIds != null && dto.ColorIds.Any())
            {


                foreach (var colorId in dto.ColorIds)
                {
                    product.AvailableColors.Add(new ProductColors
                    {
                        ColorId = colorId
                    });
                }
            }

            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync();

            var savedProduct = await _repository.GetAsync(
                p => p.Id == product.Id,
                include: q => q
                    .Include(p => p.Category)

                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            return new ApiResponse<ProductDto>(
                200,
                "Product created successfully",
                MapToDTO(savedProduct!)
            );
        }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto dto)
        {
            if (dto == null)
                return new ApiResponse<ProductDto>(400, "Invalid product data");

            var product = await _repository.GetAsync(
                p => p.Id == dto.Id,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            if (product == null)
                return new ApiResponse<ProductDto>(404, "Product not found");



            if (product.IsDeleted)
                return new ApiResponse<ProductDto>(400, "Cannot update a deleted product");

            var trimmedName = dto.Name.Trim();

            var nameExists = await _repository.GetAsync(
                p => !p.IsDeleted &&
                     p.Id != dto.Id &&
                     p.Name.ToLower() == trimmedName.ToLower()
            );

            if (nameExists != null)
                return new ApiResponse<ProductDto>(409, "Product with this name already exists");

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null || category.IsDeleted)
                return new ApiResponse<ProductDto>(404, "Category not found");

            if (!category.IsActive)
                return new ApiResponse<ProductDto>(400, "Category is inactive");

            if (dto.ColorIds != null && dto.ColorIds.Any())
            {
                if (dto.ColorIds.Count != dto.ColorIds.Distinct().Count())
                    return new ApiResponse<ProductDto>(400, "Duplicate colors are not allowed");

                var colors = await _colorRepository.GetByIdsAsync(dto.ColorIds);

                if (colors.Count != dto.ColorIds.Count)
                    return new ApiResponse<ProductDto>(400, "One or more colors not found");

                if (colors.Any(c => c.IsDeleted))
                    return new ApiResponse<ProductDto>(400, "One or more colors are deleted");

                if (colors.Any(c => !c.IsActive))
                    return new ApiResponse<ProductDto>(400, "One or more colors are inactive");

                product.AvailableColors.Clear();

                foreach (var colorId in dto.ColorIds)
                {
                    product.AvailableColors.Add(new ProductColors
                    {
                        ColorId = colorId
                    });
                }
            }









            product.Name = dto.Name.Trim();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.CurrentStock = dto.CurrentStock;
            product.InStock = dto.CurrentStock > 0;
            product.IsActive = dto.IsActive;
            product.TopSelling = dto.TopSelling;
            product.Status = dto.Status;
            product.Warranty = dto.Warranty;

            //product.AvailableColors.Clear();


            _repository.Update(product);
            await _repository.SaveChangesAsync();

            var updatedProduct = await _repository.GetAsync(
     p => p.Id == product.Id,
     include: q => q
         .Include(p => p.Category)
         .Include(p => p.AvailableColors)
         .ThenInclude(pc => pc.Color)
 );

            return new ApiResponse<ProductDto>(
                200,
                "Product updated successfully",
                MapToDTO(updatedProduct!)
            );
        }






        public async Task<ApiResponse<ProductDto>> PatchProductAsync(int id, PatchProductDto dto)
        {
            var product = await _repository.GetAsync(
                p => p.Id == id,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            if (product == null)
                return new ApiResponse<ProductDto>(404, "Product not found");

            if (product.IsDeleted)
                return new ApiResponse<ProductDto>(400, "Cannot update a deleted product");

            bool changed = false;

            // 🔹 Name
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var trimmedName = dto.Name.Trim();

                if (!product.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase))
                {
                    var exists = await _repository.GetAsync(
                        p => !p.IsDeleted &&
                             p.Id != id &&
                             p.Name.ToLower() == trimmedName.ToLower()
                    );

                    if (exists != null)
                        return new ApiResponse<ProductDto>(409, "Product with this name already exists");

                    product.Name = trimmedName;
                    changed = true;
                }
            }

            // 🔹 Description
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                var desc = dto.Description.Trim();
                if (product.Description != desc)
                {
                    product.Description = desc;
                    changed = true;
                }
            }

            // 🔹 Price
            if (dto.Price.HasValue && product.Price != dto.Price.Value)
            {
                product.Price = dto.Price.Value;
                changed = true;
            }

            // 🔹 Category
            if (dto.CategoryId.HasValue && product.CategoryId != dto.CategoryId.Value)
            {
                var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value);

                if (category == null || category.IsDeleted)
                    return new ApiResponse<ProductDto>(404, "Category not found");

                if (!category.IsActive)
                    return new ApiResponse<ProductDto>(400, "Category is inactive");

                product.CategoryId = dto.CategoryId.Value;
                changed = true;
            }

            // 🔹 Stock
            if (dto.CurrentStock.HasValue && product.CurrentStock != dto.CurrentStock.Value)
            {
                product.CurrentStock = dto.CurrentStock.Value;
                product.InStock = dto.CurrentStock.Value > 0;
                changed = true;
            }

            // 🔹 Flags
            if (dto.IsActive.HasValue && product.IsActive != dto.IsActive.Value)
            {
                product.IsActive = dto.IsActive.Value;
                changed = true;
            }

            if (dto.TopSelling.HasValue && product.TopSelling != dto.TopSelling.Value)
            {
                product.TopSelling = dto.TopSelling.Value;
                changed = true;
            }

            if (dto.Status.HasValue && product.Status != dto.Status.Value)
            {
                product.Status = dto.Status.Value;
                changed = true;
            }

            // 🔹 Colors (only if provided)
            if (dto.ColorIds != null)
            {
                var existing = product.AvailableColors
                    .Select(pc => pc.ColorId)
                    .OrderBy(x => x)
                    .ToList();

                var incoming = dto.ColorIds
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                if (!existing.SequenceEqual(incoming))
                {
                    var colors = await _colorRepository.GetByIdsAsync(incoming);

                    if (colors.Count != incoming.Count)
                        return new ApiResponse<ProductDto>(400, "One or more colors not found");

                    if (colors.Any(c => c.IsDeleted))
                        return new ApiResponse<ProductDto>(400, "One or more colors are deleted");

                    if (colors.Any(c => !c.IsActive))
                        return new ApiResponse<ProductDto>(400, "One or more colors are inactive");

                    product.AvailableColors.Clear();

                    foreach (var colorId in incoming)
                    {
                        product.AvailableColors.Add(new ProductColors
                        {
                            ColorId = colorId
                        });
                    }

                    changed = true;
                }
            }

            // 🔹 Warranty
            if (dto.Warranty != null && product.Warranty != dto.Warranty)
            {
                product.Warranty = dto.Warranty;
                changed = true;
            }

            // 🟡 No changes
            if (!changed)
            {
                return new ApiResponse<ProductDto>(
                    200,
                    "No changes detected",
                    MapToDTO(product)
                );
            }

            _repository.Update(product);
            await _repository.SaveChangesAsync();

            // Reload for response
            var updatedProduct = await _repository.GetAsync(
                p => p.Id == id,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            return new ApiResponse<ProductDto>(
                200,
                "Product updated successfully",
                MapToDTO(updatedProduct!)
            );
        }










        public async Task<ApiResponse<string>> DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                return new ApiResponse<string>(404, "Product is not found");

            if (product.IsDeleted)
                return new ApiResponse<string>(400, "Product is not found");

            product.IsDeleted = true;
            product.IsActive = false;

            _repository.Update(product);
            await _repository.SaveChangesAsync();

            return new ApiResponse<string>(
                200,
                "Product deleted successfully"
            );
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetAsync(
                p => p.Id == id,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
                    .Where(p=> !p.IsDeleted)
            );

            return product == null ? null : MapToDTO(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync(
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            return products
                .Where(p =>  !p.IsDeleted)
                .Select(MapToDTO);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetAllAsync(
                predicate: p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            return products.Select(MapToDTO);
        }

        public async Task<ApiResponse<string>> ToggleProductStatusAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                return new ApiResponse<string>(404, "Product not found");

            product.IsActive = !product.IsActive;

            _repository.Update(product);
            await _repository.SaveChangesAsync();

            return new ApiResponse<string>(
                200,
                product.IsActive ? "Product activated" : "Product deactivated"
            );
        }

        

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetFilteredProducts(
            string? name,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            int page,
            int pageSize,
            string? sortBy,
            bool descending)
        {
            Expression<Func<Product, bool>> filter = p =>
                p.IsActive &&
                !p.IsDeleted &&
                (string.IsNullOrEmpty(name) || p.Name.Contains(name)) &&
                (!categoryId.HasValue || p.CategoryId == categoryId) &&
                (!minPrice.HasValue || p.Price >= minPrice) &&
                (!maxPrice.HasValue || p.Price <= maxPrice) &&
                (!inStock.HasValue || p.InStock == inStock);

            var products = await _productRepository.GetAllAsync(
                predicate: filter,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)

                    .ThenInclude(pc => pc.Color)
            );

            return new ApiResponse<IEnumerable<ProductDto>>(
                200,
                "Products fetched",
                products.Select(MapToDTO)
            );
        }
        public async Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(
    int page = 1,
    int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _productRepository.Query()
             .Where(p => p.IsActive && !p.IsDeleted)
             .Include(p => p.Category)
             .Include(p => p.AvailableColors)
                 .ThenInclude(pc => pc.Color);


            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<ProductDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = products.Select(MapToDTO)
            };

            return new ApiResponse<PagedResult<ProductDto>>(
                200,
                "Products fetched successfully",
                result
            );
        }
        private static ProductDto MapToDTO(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                CurrentStock = p.CurrentStock,
                InStock = p.InStock,
                IsActive = p.IsActive,

                //AvailableColors = p.AvailableColors
                //    .Where(pc => pc.Color != null)
                //    .Select(pc => pc.Color!.Name)
                //    .ToList()
                AvailableColors = p.AvailableColors
    .Where(pc => pc.Color != null &&
        pc.Color.IsActive &&
        !pc.Color.IsDeleted
    )
    .Select(pc => pc.Color.Name)
    .ToList(),

                DeactivatedColors = p.AvailableColors
    .Where(pc => pc.Color != null &&
       ( !pc.Color.IsActive ||
        pc.Color.IsDeleted)
    )
    .Select(pc => pc.Color.Name)
    .ToList()




            };
        }
        

    }
}
