using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Application.DTOs.ColorDTOs;
using Dominus.Domain.Entities;
using System.Text.RegularExpressions;

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


        private static string NormalizeHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex code cannot be empty");

            hex = hex.Trim();

            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            if (!Regex.IsMatch(hex, @"^#[0-9a-fA-F]{6}$"))
                throw new ArgumentException("Hex code must be in #RRGGBB format");

            return hex.ToLowerInvariant();
        }






        public async Task<ApiResponse<ColorDto>> CreateAsync(CreateColorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new ApiResponse<ColorDto>(400, "Color name cannot be empty");

            if (string.IsNullOrWhiteSpace(dto.HexCode))
                return new ApiResponse<ColorDto>(400, "Hex code cannot be empty");

            var name = dto.Name.Trim(); 
            var normalizedName = name.ToLower();
            string hex;
            try
            {
                hex = NormalizeHex(dto.HexCode);
            }
            catch (ArgumentException ex)
            {
                return new ApiResponse<ColorDto>(400, ex.Message);
            }
            var nameExists = await _repository.GetByNameAsync(normalizedName);
            if (nameExists != null)
                return new ApiResponse<ColorDto>(409, "Color name already exists");

            var hexExists = await _repository.GetByHexAsync(hex);
            if (hexExists != null)
                return new ApiResponse<ColorDto>(409, "Hex code already exists");

            var color = new Color
            {
                Name = name,
                HexCode = hex,
                IsActive = true
            };

            await _repository.AddAsync(color);
            await _repository.SaveChangesAsync();

            return new ApiResponse<ColorDto>(200, "Color created", Map(color));
        }


        public async Task<ApiResponse<ColorDto>> UpdateAsync(UpdateColorDto dto)
        {
            if (dto.Id <= 0)
                return new ApiResponse<ColorDto>(400, "Invalid color id");

            var color = await _repository.GetByIdAsync(dto.Id);
            if (color == null || color.IsDeleted)
                return new ApiResponse<ColorDto>(404, "Color not found");

            // 🔹 NAME update + uniqueness (exclude same ID)
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var trimmedName = dto.Name.Trim();

                var existingName = await _repository.GetByNameAsync(trimmedName);
                if (existingName != null && existingName.Id != dto.Id)
                    return new ApiResponse<ColorDto>(409, "Color name already exists");

                color.Name = trimmedName;
            }

            // 🔹 HEX update + normalize + uniqueness (exclude same ID)
            if (!string.IsNullOrWhiteSpace(dto.HexCode))
            {
                string normalizedHex;

                try
                {
                    normalizedHex = NormalizeHex(dto.HexCode);
                }
                catch (ArgumentException ex)
                {
                    return new ApiResponse<ColorDto>(400, ex.Message);
                }

                var existingHex = await _repository.GetByHexAsync(normalizedHex);
                if (existingHex != null && existingHex.Id != dto.Id)
                    return new ApiResponse<ColorDto>(409, "Hex code already exists");

                color.HexCode = normalizedHex;
            }

            // 🔹 Status update
            if (dto.IsActive.HasValue)
            {
                color.IsActive = dto.IsActive.Value;
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
            color.IsActive = false;
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
