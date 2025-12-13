using BCrypt.Net;
using Dominus.Domain.DTOs.AuthDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dominus.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IGenericRepository<User> userRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetAsync(u => u.Email == registerRequestDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto(400, "User with this email already exists");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequestDto.Password);

            // Create new user
            var user = new User
            {
                Name = registerRequestDto.Name,
                Email = registerRequestDto.Email,
                PasswordHash = passwordHash,
                Role = Roles.user,
                IsBlocked = false,
                CreatedOn = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(201, "User registered successfully");
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            // Find user by email
            var user = await _userRepository.GetAsync(u => u.Email == loginRequestDto.Email);
            if (user == null)
            {
                return new AuthResponseDto(404, "User not found");
            }

            // Check if user is blocked
            if (user.IsBlocked)
            {
                return new AuthResponseDto(403, "User account is blocked");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.PasswordHash))
            {
                return new AuthResponseDto(401, "Invalid credentials");
            }

            // Generate tokens
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(200, "Login successful", accessToken, refreshToken);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // Find user by refresh token
            var user = await _userRepository.GetAsync(u => u.RefreshToken == refreshToken);
            if (user == null)
            {
                return new AuthResponseDto(401, "Invalid refresh token");
            }

            // Check if refresh token is expired
            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return new AuthResponseDto(401, "Refresh token has expired");
            }

            // Check if user is blocked
            if (user.IsBlocked)
            {
                return new AuthResponseDto(403, "User account is blocked");
            }

            // Generate new tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Update refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(200, "Token refreshed successfully", newAccessToken, newRefreshToken);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetAsync(u => u.RefreshToken == refreshToken);
            if (user == null)
            {
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),

                new Claim(ClaimTypes.Role, user.Role.ToString().ToLower())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(360),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
