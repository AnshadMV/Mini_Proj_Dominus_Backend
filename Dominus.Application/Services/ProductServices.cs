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
            if (dto == null)
                return new ApiResponse<ProductDto>(400, "Invalid product data");

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null)
                return new ApiResponse<ProductDto>(404, "Category not found");

            if (!category.IsActive)
                return new ApiResponse<ProductDto>(400, "Category is inactive");

            var colors = new List<Color>();

            if (dto.ColorIds != null && dto.ColorIds.Any())
            {
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
                201,
                "Product created successfully",
                MapToDTO(savedProduct!)
            );
        }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductDto dto)
        {
            var product = await _repository.GetAsync(
                p => p.Id == dto.Id,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            if (product == null)
                return new ApiResponse<ProductDto>(404, "Product not found");

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

            product.AvailableColors.Clear();

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

            _repository.Update(product);
            await _repository.SaveChangesAsync();

            return new ApiResponse<ProductDto>(
                200,
                "Product updated successfully",
                MapToDTO(product)
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
                .Where(p => p.IsActive && !p.IsDeleted)
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

        // ================= MAPPER =================
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

                AvailableColors = p.AvailableColors
                    .Where(pc => pc.Color != null)
                    .Select(pc => pc.Color!.Name)
                    .ToList()
            };
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

    }
}
