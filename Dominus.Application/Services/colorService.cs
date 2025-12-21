using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ColorDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;

namespace Dominus.Application.Services
{
    public class ColorService : IColorService
    {
        private readonly IColorRepository _repository;
        //private readonly IGenericRepository<Color> _genericRepo;

        public ColorService(IColorRepository repository, IGenericRepository<Color> genericRepo)
        {
            _repository = repository;
            //_genericRepo = genericRepo;
        }

        public async Task<ApiResponse<ColorDto>> CreateAsync(CreateColorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new ApiResponse<ColorDto>(400, "Color name cannot be empty");

            if (string.IsNullOrWhiteSpace(dto.HexCode))
                return new ApiResponse<ColorDto>(400, "Hex code cannot be empty");

            var name = dto.Name.Trim();
            var hex = dto.HexCode.Trim();

            var exists = await _repository.GetByNameAsync(name);
            if (exists != null)
                return new ApiResponse<ColorDto>(409, "Color already exists");

            var color = new Color
            {
                Name = name,
                HexCode = hex,
                IsActive = true
                // ✅ audit fields handled by DB defaults
            };

            await _repository.AddAsync(color);
            await _repository.SaveChangesAsync();

            return new ApiResponse<ColorDto>(200, "Color created", Map(color));
        }


        public async Task<ApiResponse<ColorDto>> UpdateAsync(UpdateColorDto dto)
        {
            var color = await _repository.GetByIdAsync(dto.Id);
            if (color == null)
                return new ApiResponse<ColorDto>(409, "Color not found");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                color.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.HexCode))
                color.HexCode = dto.HexCode.Trim();

            if (dto.IsActive.HasValue)
            {
                color.IsActive = dto.IsActive.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var trimmedName = dto.Name.Trim();

                var existing = await _repository.GetByNameAsync(trimmedName);

                if (existing != null && existing.Id != dto.Id)
                    return new ApiResponse<ColorDto>(409, "Color name already exists");

                color.Name = trimmedName;
            }

            if (!string.IsNullOrWhiteSpace(dto.HexCode))
            {
                color.HexCode = dto.HexCode.Trim();
            }

            _repository.Update(color);
            await _repository.SaveChangesAsync();

            return new ApiResponse<ColorDto>(200, "Color updated", Map(color));
        }


        public async Task<ApiResponse<IEnumerable<ColorDto>>> GetAllAsync()
        {
            var colors = await _repository.GetAllAsync();
            return new ApiResponse<IEnumerable<ColorDto>>(
                200,
                "Colors fetched",
                colors.Where(c => !c.IsDeleted).Select(Map)
            );
        }

        public async Task<ApiResponse<string>> ToggleStatusAsync(int id)
        {
            if (id <= 0)
                return new ApiResponse<string>(400, "Invalid color id");

            var color = await _repository.GetByIdAsync(id);

            if (color == null)
                return new ApiResponse<string>(404, "Color not found");

            if (color.IsDeleted)
                return new ApiResponse<string>(400, "Cannot change status of a deleted color");

            color.IsActive = !color.IsActive;

            _repository.Update(color);
            await _repository.SaveChangesAsync();

            return new ApiResponse<string>(
                201,
                color.IsActive ? "Color activated successfully" : "Color deactivated successfully"
            );
        }






        public async Task<ApiResponse<string>> SoftDeleteColorAsync(int id)
        {
            var color = await _repository.GetByIdAsync(id);
            if (color == null || color.IsDeleted)
                return new ApiResponse<string>(404, "Color not found");

            //if (color.Id != null && color..Any(p => !p.IsDeleted))
            //    return new ApiResponse<string>(400, "Cannot delete color with existing products");

            _repository.Delete(color);
            await _repository.SaveChangesAsync();

            return new ApiResponse<string>(201, "Colors deleted", "deleted");
        }

        private static ColorDto Map(Color c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            HexCode = c.HexCode,
            IsActive = c.IsActive
        };

        
    }

}
