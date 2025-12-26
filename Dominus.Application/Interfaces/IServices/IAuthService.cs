using Dominus.Application.DTOs.UserProfile;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.AuthDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
        Task<AuthResponseDto> LogoutAsync(string refreshToken);
        Task<AuthResponseDto> UpdateProfileAsync(UpdateProfileRequestDto dto, int userId);
        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(int userId);

        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<AuthResponseDto> GenerateAccessTokenFromRefreshAsync(string refreshToken);
        Task<AuthResponseDto> ForgotPasswordAsync(string email);
        Task<AuthResponseDto> ResetPasswordAsync(string token, string newPassword);

    }
}
