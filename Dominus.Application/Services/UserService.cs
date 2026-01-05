using Dominus.Application.DTOs.UserProfile;
using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Domain.Entities;
using Dominus.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dominus.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUserRepository _userRepositoryExtended;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public UserService(
            IGenericRepository<User> userRepository,
            IUserRepository userRepositoryExtended, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _userRepository = userRepository;
            _userRepositoryExtended = userRepositoryExtended;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;

        }

        public async Task<ApiResponse<IEnumerable<User>>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync(u => !u.IsDeleted);

            return new ApiResponse<IEnumerable<User>>(
                200,
                "Users retrieved successfully",
                users
            );
        }

        public async Task<ApiResponse<User>> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
                throw new KeyNotFoundException("User not found");

            return new ApiResponse<User>(
                200,
                "User retrieved successfully",
                user
            );
        }

        public async Task<ApiResponse<string>> BlockUnblockUserAsync(int id)
        {
            var userPrincipal = _httpContextAccessor.HttpContext?.User;

            if (userPrincipal == null || !userPrincipal.Identity!.IsAuthenticated)
                throw new UnauthorizedAccessException("Unauthorized");

            var adminIdClaim = userPrincipal.FindFirst("userId");

            if (adminIdClaim == null)
                throw new UnauthorizedAccessException("Unauthorized");

            var adminId = int.Parse(adminIdClaim.Value);

            var admin = await _userRepository.GetByIdAsync(adminId);

            if (admin == null || admin.IsDeleted)
                throw new KeyNotFoundException("Admin not found");

            if (admin.IsBlocked)
                throw new UnauthorizedAccessException(
                    "Blocked admins cannot block or unblock users"
                );

            var targetUser = await _userRepository.GetByIdAsync(id);

            if (targetUser == null || targetUser.IsDeleted)
                throw new KeyNotFoundException("User not found");
            if (targetUser.Role == Roles.admin)
                return new ApiResponse<string>(
                    403,
                    "Admin accounts cannot be blocked or unblocked",
                    "forbidden"
                );
            await _userRepositoryExtended.BlockUnblockUserAsync(id);

            var action = targetUser.IsBlocked ? "Blocked" : "Un-Blocked";

            return new ApiResponse<string>(
                200,
                $"User {action} successfully",
                action
            );
        }



        public async Task<ApiResponse<string>> SoftDeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
                throw new KeyNotFoundException("User not found");

            await _userRepositoryExtended.SoftDeleteUserAsync(id);

            return new ApiResponse<string>(
                200,
                "User deleted successfully",
                "deleted"
            );
        }



      



    }
}
