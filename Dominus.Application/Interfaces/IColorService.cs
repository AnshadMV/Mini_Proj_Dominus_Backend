using Dominus.Domain.Common;
using Dominus.Domain.DTOs.ColorDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Interfaces
{
    public interface IColorService
    {
        Task<ApiResponse<ColorDto>> CreateAsync(CreateColorDto dto);
        Task<ApiResponse<ColorDto>> UpdateAsync(UpdateColorDto dto);
        Task<ApiResponse<IEnumerable<ColorDto>>> GetAllAsync();
        Task<ApiResponse<string>> ToggleStatusAsync(int id);
    }

}
