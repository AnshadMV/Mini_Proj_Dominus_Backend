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
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || user.IsDeleted)
                throw new KeyNotFoundException("User not found");

            await _userRepositoryExtended.BlockUnblockUserAsync(id);

            var action = user.IsBlocked ? "unblocked" : "blocked";

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
