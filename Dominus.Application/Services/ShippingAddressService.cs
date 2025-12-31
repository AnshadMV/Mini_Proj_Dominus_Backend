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
            await _repo.DeactivateAllAsync(userId);
            var address = new ShippingAddress
            {
                UserId = userId,
                AddressLine = dto.AddressLine.Trim(),
                City = dto.City.Trim(),
                State = dto.State.Trim(),
                Pincode = dto.Pincode,
                Phone = dto.Phone,
                IsActive = true
            };

            await _repo.AddAsync(address);

            return new ApiResponse<string>(201, "Address added successfully", "created");
        }

        public async Task<ApiResponse<IEnumerable<ShippingAddressDto>>> GetMyAddressesAsync()
        {
            var userId = GetUserId();

            var addresses = await _repo.GetByUserIdAsync(userId);

            var result = addresses
         .OrderByDescending(a => a.IsActive) 
         .Select(a => new ShippingAddressDto
         {
             Id = a.Id,
             AddressLine = a.AddressLine,
             City = a.City,
             State = a.State,
             Pincode = a.Pincode,
             Phone = a.Phone,
             IsActive = a.IsActive
         });

            return new ApiResponse<IEnumerable<ShippingAddressDto>>(200, "Addresses fetched", result);
        }
        public async Task<ApiResponse<string>> UpdateAsync(
    int addressId,
    UpdateShippingAddressRequestDto dto)
        {
            var userId = GetUserId();

            var address = await _repo.GetByIdAsync(addressId);

            if (address == null || address.IsDeleted)
                throw new KeyNotFoundException("Shipping address not found");

            if (address.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to update this address"
                );

            address.AddressLine = dto.AddressLine.Trim();
            address.City = dto.City.Trim();
            address.State = dto.State.Trim();
            address.Pincode = dto.Pincode;
            address.Phone = dto.Phone;

            await _repo.UpdateAsync(address);

            return new ApiResponse<string>(
                200,
                "Shipping address updated successfully",
                "updated"
            );
        }
        public async Task<ApiResponse<string>> SetActiveAsync(int addressId)
        {
            var userId = GetUserId();

            var address = await _repo.GetByIdAsync(addressId);

            if (address == null || address.IsDeleted)
                throw new KeyNotFoundException("Shipping address not found");

            if (address.UserId != userId)
                throw new UnauthorizedAccessException("Not allowed");

            // 🔴 Deactivate all
            await _repo.DeactivateAllAsync(userId);

            // 🟢 Activate selected
            address.IsActive = true;
            await _repo.UpdateAsync(address);

            return new ApiResponse<string>(
                200,
                "Shipping address set as active",
                "active"
            );
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
