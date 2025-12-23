using Dominus.Domain.Entities;
using Dominus.Domain.Common;
namespace Dominus.Application.Interfaces.IServices
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<User>>> GetAllUsersAsync();
        Task<ApiResponse<User>> GetUserByIdAsync(int id);

        Task<ApiResponse<string>> BlockUnblockUserAsync(int id);
        Task<ApiResponse<string>> SoftDeleteUserAsync(int id);
    }
}
