using Dominus.Application.DTOs.ShippingAddress;
using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dominus.Application.Services
{
    public class ShippingAddressService : IShippingAddressService
    {
        private readonly IShippingAddressRepository _repo;
        private readonly IHttpContextAccessor _http;

        public ShippingAddressService(
            IShippingAddressRepository repo,
            IHttpContextAccessor http)
        {
            _repo = repo;
            _http = http;
        }

        private int GetUserId()
        {
            var claim = _http.HttpContext?.User.FindFirst("userId");
            if (claim == null) throw new UnauthorizedAccessException("Unauthorized");
            return int.Parse(claim.Value);
        }

        public async Task<ApiResponse<string>> AddAsync(int userId, ShippingAddressRequestDto dto)
        {
            var address = new ShippingAddress
            {
                UserId = userId,
                AddressLine = dto.AddressLine.Trim(),
                City = dto.City.Trim(),
                State = dto.State.Trim(),
                Pincode = dto.Pincode,
                Phone = dto.Phone
            };

            await _repo.AddAsync(address);

            return new ApiResponse<string>(201, "Address added successfully", "created");
        }

        public async Task<ApiResponse<IEnumerable<ShippingAddressDto>>> GetMyAddressesAsync()
        {
            var userId = GetUserId();

            var addresses = await _repo.GetByUserIdAsync(userId);

            var result = addresses.Select(a => new ShippingAddressDto
            {
                Id = a.Id,
                AddressLine = a.AddressLine,
                City = a.City,
                State = a.State,
                Pincode = a.Pincode,
                Phone = a.Phone
            });

            return new ApiResponse<IEnumerable<ShippingAddressDto>>(200, "Addresses fetched", result);
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var userId = GetUserId();
            var address = await _repo.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
                throw new KeyNotFoundException("Address not found");

            await _repo.SoftDeleteAsync(address);

            return new ApiResponse<string>(200, "Address deleted", "deleted");
        }
    }
}
