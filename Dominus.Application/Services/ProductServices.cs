using Dominus.Application.DTOs.P_ImageDTOs;
using Dominus.Application.DTOs.ProductDTOs;
using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Application.DTOs.ProductDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Entities.Dominus.Domain.Entities;
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
        private readonly IImageStorageService _imageService;

        public ProductService(
            IGenericRepository<Product> repository,
            IProductRepository productRepository,
            IColorRepository colorRepository,
           ICategoryRepository categoryRepository,
            IImageStorageService imageService)
        {
            _repository = repository;
            _productRepository = productRepository;
            _colorRepository = colorRepository;
            _categoryRepository = categoryRepository;
            _imageService = imageService;
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
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var file in dto.Images)
                {
                    var (url, publicId) = await _imageService.UploadAsync(file);

                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = url,
                        PublicId = publicId
                    });
                }
            }
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
                       .Include(p => p.Images)
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
                    .Include(p => p.Category).Include(p => p.Images)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            if (product == null)
                return new ApiResponse<ProductDto>(404, "Product not found");



            if (product.IsDeleted)
                return new ApiResponse<ProductDto>(404, "Product not found");

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
                    return new ApiResponse<ProductDto>(409, "Duplicate colors are not allowed");

                var colors = await _colorRepository.GetByIdsAsync(dto.ColorIds);

                if (colors.Count != dto.ColorIds.Count)
                    return new ApiResponse<ProductDto>(404, "Colors not found");

                if (colors.Any(c => c.IsDeleted))
                    return new ApiResponse<ProductDto>(404, "Colors not found");

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
         .Include(p => p.Category).Include(p => p.Images)
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
                    .Include(p => p.Category).Include(p => p.Images)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            if (product == null)
                return new ApiResponse<ProductDto>(404, "Product not found");

            if (product.IsDeleted)
                return new ApiResponse<ProductDto>(400, "Cannot update a deleted product");

            bool changed = false;

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

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                var desc = dto.Description.Trim();
                if (product.Description != desc)
                {
                    product.Description = desc;
                    changed = true;
                }
            }

            if (dto.Price.HasValue && product.Price != dto.Price.Value)
            {
                product.Price = dto.Price.Value;
                changed = true;
            }

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

            if (dto.CurrentStock.HasValue && product.CurrentStock != dto.CurrentStock.Value)
            {
                product.CurrentStock = dto.CurrentStock.Value;
                product.InStock = dto.CurrentStock.Value > 0;
                changed = true;
            }

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
                        return new ApiResponse<ProductDto>(404, "One or more colors not found");

                    if (colors.Any(c => c.IsDeleted))
                        return new ApiResponse<ProductDto>(404, "One or more colors not found");

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

            if (dto.Warranty != null && product.Warranty != dto.Warranty)
            {
                product.Warranty = dto.Warranty;
                changed = true;
            }

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

            var updatedProduct = await _repository.GetAsync(
                p => p.Id == id,
                include: q => q
                    .Include(p => p.Category).Include(p => p.Images)
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
                return new ApiResponse<string>(404, "Product is not found");

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
                    .Include(p => p.Category).Include(p => p.Images)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
                    .Where(p=> !p.IsDeleted)
            );

            return product == null ? null : MapToDTO(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            return await _productRepository.Query()
    .Where(p => !p.IsDeleted)
    .AsNoTracking()
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Description = p.Description,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        CurrentStock = p.CurrentStock,
        InStock = p.InStock,
        IsActive = p.IsActive,
        TopSelling = p.TopSelling,
        Warranty = p.Warranty,
        Images = p.Images
            .Where(i => !i.IsDeleted)
            .Select(i => i.ImageUrl)
            .ToList(),
        AvailableColors = p.AvailableColors
            .Where(c => c.Color.IsActive && !c.Color.IsDeleted)
            .Select(c => c.Color.Name)
            .ToList()
    })
    .ToListAsync();

        }



        public async Task<IEnumerable<ProductDto>> GetAllProductsUserAsync()
        {
            return await _productRepository.Query()
    .Where(p => !p.IsDeleted & !p.IsActive )
    .AsNoTracking()
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Description = p.Description,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        CurrentStock = p.CurrentStock,
        InStock = p.InStock,
        IsActive = p.IsActive,
        TopSelling = p.TopSelling,
        Warranty = p.Warranty,
        Images = p.Images
            .Where(i => !i.IsDeleted)
            .Select(i => i.ImageUrl)
            .ToList(),
        AvailableColors = p.AvailableColors
            .Where(c => c.Color.IsActive && !c.Color.IsDeleted)
            .Select(c => c.Color.Name)
            .ToList()
    })
    .ToListAsync();

        }







        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetAllAsync(
                predicate: p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted,
                include: q => q
                    .Include(p => p.Category)
                     .Include(p => p.Images)
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

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetFilteredProductsAsync(
    ProductFilterDto filter)
        {
            if (filter.ProductId.HasValue && filter.ProductId.Value <= 0)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "ProductId must be greater than 0"
                );
            }

            if (filter.Page < 1)
                filter.Page = 1;

            if (filter.PageSize < 1 || filter.PageSize > 1000)
                filter.PageSize = 20;

            if (filter.MinPrice.HasValue && filter.MinPrice.Value <= 0)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "MinPrice must be greater than 0"
                );
            }

            if (filter.MaxPrice.HasValue && filter.MaxPrice.Value <= 0)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "MaxPrice must be greater than 0"
                );
            }

            if (filter.MinPrice.HasValue &&
                filter.MaxPrice.HasValue &&
                filter.MinPrice.Value > filter.MaxPrice.Value)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "MinPrice cannot be greater than MaxPrice"
                );
            }

            if (filter.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(filter.CategoryId.Value);

                if (category == null || category.IsDeleted || !category.IsActive)
                    return new ApiResponse<PagedResult<ProductDto>>(
                        404, "Category not found or inactive");
            }

            if (filter.ColorId.HasValue && filter.ColorId.Value <= 0)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "ColorId must be greater than 0"
                );
            }

            if (filter.ColorId.HasValue)
            {
                var color = await _colorRepository.GetByIdAsync(filter.ColorId.Value);

                if (color == null || color.IsDeleted || !color.IsActive)
                {
                    return new ApiResponse<PagedResult<ProductDto>>(
                        404,
                        "Color not found or inactive"
                    );
                }
            }

            string? normalizedName = null;

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                normalizedName = filter.Name.Trim();
            }

            IQueryable<Product> query;

            query = _productRepository.Query()
                .Where(p => !p.IsDeleted);

            if (filter.ProductId.HasValue)
            {
                query = query.Where(p => p.Id == filter.ProductId.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }


            query = query.Where(p =>
                    (normalizedName == null ||
                        EF.Functions.Like(p.Name, $"%{normalizedName}%", @"\")) &&
                    (!filter.CategoryId.HasValue || p.CategoryId == filter.CategoryId) &&
                     (!filter.ColorId.HasValue ||
        p.AvailableColors.Any(pc => pc.ColorId == filter.ColorId.Value)) &&
                    (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice) &&
                    (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice) &&
                    (!filter.InStock.HasValue || p.InStock == filter.InStock)
                )
                .Include(p => p.Category).Include(p => p.Images)
                .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color);



            query = filter.SortBy?.ToLower() switch
            {
                "price" => filter.Descending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),

                "name" => filter.Descending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),

                "createdon" or _ => filter.Descending
                    ? query.OrderByDescending(p => p.CreatedOn)
                    : query.OrderBy(p => p.CreatedOn)
            };

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    404,
                    "No products founded for the given filters"
                );
            }
            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new PagedResult<ProductDto>
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount,
                Items = products.Select(MapToDTO)
            };

            return new ApiResponse<PagedResult<ProductDto>>(
                200,
                "Products fetched successfully",
                result
            );
        }


        //public async Task<ApiResponse<IEnumerable<ProductDto>>> GetFilteredProducts(
        //    string? name,
        //    int? categoryId,
        //    decimal? minPrice,
        //    decimal? maxPrice,
        //    bool? inStock,
        //    int page,
        //    int pageSize,
        //    string? sortBy,
        //    bool descending)
        //{
        //    Expression<Func<Product, bool>> filter = p =>
        //        p.IsActive &&
        //        !p.IsDeleted &&
        //        (string.IsNullOrEmpty(name) || p.Name.Contains(name)) &&
        //        (!categoryId.HasValue || p.CategoryId == categoryId) &&
        //        (!minPrice.HasValue || p.Price >= minPrice) &&
        //        (!maxPrice.HasValue || p.Price <= maxPrice) &&
        //        (!inStock.HasValue || p.InStock == inStock);

        //    var products = await _productRepository.GetAllAsync(
        //        predicate: filter,
        //        include: q => q
        //            .Include(p => p.Category)
        //            .Include(p => p.AvailableColors)

        //            .ThenInclude(pc => pc.Color)
        //    );

        //    return new ApiResponse<IEnumerable<ProductDto>>(
        //        200,
        //        "Products fetched",
        //        products.Select(MapToDTO)
        //    );
        //}
        public async Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(
      int page = 1,
      int pageSize = 10)
        {
            if (page < 1)
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "Page number must be greater than or equal to 1"
                );

            if (pageSize < 1)
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "Page size must be greater than or equal to 1"
                );

            const int maxPageSize = 100;
            if (pageSize > maxPageSize)
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    $"Page size cannot exceed {maxPageSize}"
                );

            var query = _productRepository.Query()
                .Where(p => p.IsActive && !p.IsDeleted)
                .Include(p => p.Category).Include(p => p.Images)
                .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    200,
                    "No products found",
                    new PagedResult<ProductDto>
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = 0,
                        Items = Enumerable.Empty<ProductDto>()
                    }
                );
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page > totalPages)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    $"Page number exceeds total pages ({totalPages})"
                );
            }

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

        

        public async Task<ApiResponse<ProductDto>> AddStockAsync(AddProductStockDto dto)
        {
            if (dto == null)
                return new ApiResponse<ProductDto>(400, "Invalid stock data");

            var product = await _repository.GetByIdAsync(dto.ProductId);

            if (product == null || product.IsDeleted)
                return new ApiResponse<ProductDto>(404, "Product not found");

            if (!product.IsActive)
                return new ApiResponse<ProductDto>(
                    400,
                    "Cannot add stock to an inactive product"
                );

            const int maxStockLimit = 1_000_000;

            if (product.CurrentStock + dto.QuantityToAdd > maxStockLimit)
                return new ApiResponse<ProductDto>(
                    400,
                    $"Stock limit exceeded. Max allowed: {maxStockLimit}"
                );

            product.CurrentStock += dto.QuantityToAdd;
            product.InStock = product.CurrentStock > 0;
            product.ModifiedOn = DateTime.UtcNow;

            _repository.Update(product);
            await _repository.SaveChangesAsync();

            var updatedProduct = await _repository.GetAsync(
                p => p.Id == product.Id,
                include: q => q
                    .Include(p => p.Category).Include(p => p.Images)
                    .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color)
            );

            return new ApiResponse<ProductDto>(
                200,
                $"Stock added successfully (+{dto.QuantityToAdd})",
                MapToDTO(updatedProduct!)
            );
        }


        public async Task<ApiResponse<PagedResult<ProductDto>>>
    SearchProductsByNameAsync(string search, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    400,
                    "Search keyword cannot be empty"
                );
            }

            search = search.Trim();

            if (page < 1)
                return new ApiResponse<PagedResult<ProductDto>>(400, "Page must be >= 1");

            if (pageSize < 1 || pageSize > 100)
                return new ApiResponse<PagedResult<ProductDto>>(400, "PageSize must be between 1 and 100");

            var query = _productRepository.Query()
                .Where(p =>
                    !p.IsDeleted &&
                    
                    EF.Functions.Like(p.Name, $"%{search}%"))
                .Include(p => p.Category).Include(p => p.Images)
                .Include(p => p.AvailableColors)
                    .ThenInclude(pc => pc.Color);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new ApiResponse<PagedResult<ProductDto>>(
                    404,
                    $"No products found matching '{search}'"
                );
            }

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
        public async Task<ApiResponse<List<string>>> AddProductImagesAsync(AddProductImagesDto dto)
        {
            var product = await _repository.GetAsync(
                p => p.Id == dto.ProductId && !p.IsDeleted,
                q => q.Include(p => p.Images)
            );

            if (product == null)
                return new ApiResponse<List<string>>(404, "Product not found");

            var uploadedUrls = new List<string>();

            foreach (var file in dto.Images)
            {
                var (url, publicId) = await _imageService.UploadAsync(file);

                product.Images.Add(new ProductImage
                {
                    ImageUrl = url,
                    PublicId = publicId
                });

                uploadedUrls.Add(url);
            }

            _repository.Update(product);
            await _repository.SaveChangesAsync();

            return new ApiResponse<List<string>>(
                200,
                "Images added successfully",
                uploadedUrls
            );
        }
        public async Task<ApiResponse<bool>> DeleteProductImageAsync(int imageId)
        {
            var image = await _repository.Query()
                .SelectMany(p => p.Images)
                .FirstOrDefaultAsync(i => i.Id == imageId && !i.IsDeleted);

            if (image == null)
                return new ApiResponse<bool>(404, "Image not found");

            await _imageService.DeleteAsync(image.PublicId);

            image.IsDeleted = true;

            _repository.Update(image.Product);
            await _repository.SaveChangesAsync();

            return new ApiResponse<bool>(200, "Image removed successfully", true);
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
                CategoryName = p.Category?.Name ?? string.Empty,
                CurrentStock = p.CurrentStock,
                InStock = p.InStock,
                IsActive = p.IsActive,
            TopSelling = p.TopSelling,
                Warranty = p.Warranty,
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
       (!pc.Color.IsActive ||
        pc.Color.IsDeleted)
    )
    .Select(pc => pc.Color.Name)
    .ToList(),

                Images = p.Images
    .Where(i => !i.IsDeleted)
    .Select(i => i.ImageUrl)
    .ToList(),



            };
        }



    }
}
