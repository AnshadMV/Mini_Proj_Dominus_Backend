using Dominus.Domain.Common;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;

namespace Dominus.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUserRepository _userRepositoryExtended;

        public UserService(
            IGenericRepository<User> userRepository,
            IUserRepository userRepositoryExtended)
        {
            _userRepository = userRepository;
            _userRepositoryExtended = userRepositoryExtended;
        }

        public async Task<ApiResponse<IEnumerable<User>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync(u => !u.IsDeleted);
                return new ApiResponse<IEnumerable<User>>(200, "Users retrieved successfully", users);
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<User>>(500, $"Error retrieving users: {ex.Message}");
            }
        }





        public async Task<ApiResponse<User>> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return new ApiResponse<User>(404, "User not found");
                }

                return new ApiResponse<User>(200, "User retrieved successfully", user);
            }
            catch (Exception ex)
            {
                return new ApiResponse<User>(500, $"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> BlockUnblockUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return new ApiResponse<string>(404, "User not found");
                }

                await _userRepositoryExtended.BlockUnblockUserAsync(id);
                var action = user.IsBlocked ? "unblocked" : "blocked";
                return new ApiResponse<string>(200, $"User {action} successfully", action);
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(500, $"Error updating user: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> SoftDeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return new ApiResponse<string>(404, "User not found");
                }

                await _userRepositoryExtended.SoftDeleteUserAsync(id);
                return new ApiResponse<string>(200, "User deleted successfully", "deleted");
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(500, $"Error deleting user: {ex.Message}");
            }
        }
    }
}







