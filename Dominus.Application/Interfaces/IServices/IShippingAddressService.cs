using Dominus.Application.DTOs.ShippingAddress;
using Dominus.Domain.Common;

namespace Dominus.Application.Interfaces.IServices
{
    public interface IShippingAddressService
    {
        Task<ApiResponse<string>> AddAsync(int userId, ShippingAddressRequestDto dto);
        Task<ApiResponse<IEnumerable<ShippingAddressDto>>> GetMyAddressesAsync();
        Task<ApiResponse<string>> DeleteAsync(int id);
    }
}
