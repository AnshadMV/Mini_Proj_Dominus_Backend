using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominus.Domain.DTOs.AuthDTOs;

namespace Dominus.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
        Task<AuthResponseDto> LogoutAsync(string refreshToken);

        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<AuthResponseDto> GenerateAccessTokenFromRefreshAsync(string refreshToken);

    }
}
