using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ColorDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.Services
{
    public class ColorService : IColorService
    {
        private readonly IColorRepository _repository;

        public ColorService(IColorRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<ColorDto>> CreateAsync(CreateColorDto dto)
        {
            var exists = await _repository.GetByNameAsync(dto.Name);
            if (exists != null)
                return new ApiResponse<ColorDto>(400, "Color already exists");

            var color = new Color
            {
                Name = dto.Name.Trim(),
                HexCode = dto.HexCode.Trim()
            };

            await _repository.AddAsync(color);
            await _repository.SaveChangesAsync();

            return new ApiResponse<ColorDto>(201, "Color created", Map(color));
        }

        public async Task<ApiResponse<ColorDto>> UpdateAsync(UpdateColorDto dto)
        {
            var color = await _repository.GetByIdAsync(dto.Id);
            if (color == null)
                return new ApiResponse<ColorDto>(404, "Color not found");

            color.Name = dto.Name;
            color.HexCode = dto.HexCode;
            color.IsActive = dto.IsActive;

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
            var color = await _repository.GetByIdAsync(id);
            if (color == null)
                return new ApiResponse<string>(404, "Color not found");

            color.IsActive = !color.IsActive;

            _repository.Update(color);
            await _repository.SaveChangesAsync();

            return new ApiResponse<string>(
                200,
                color.IsActive ? "Color activated" : "Color deactivated"
            );
        }

        private static ColorDto Map(Color c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            HexCode = c.HexCode,
            IsActive = c.IsActive
        };

        Task<ApiResponse<ColorDto>> IColorService.CreateAsync(CreateColorDto dto)
        {
            throw new NotImplementedException();
        }

        Task<ApiResponse<ColorDto>> IColorService.UpdateAsync(UpdateColorDto dto)
        {
            throw new NotImplementedException();
        }

        Task<ApiResponse<IEnumerable<ColorDto>>> IColorService.GetAllAsync()
        {
            throw new NotImplementedException();
        }

        Task<ApiResponse<string>> IColorService.ToggleStatusAsync(int id)
        {
            throw new NotImplementedException();
        }
    }

}
