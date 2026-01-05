using BCrypt.Net;
using Dominus.Application.DTOs.UserProfile;
using Dominus.Application.Interfaces.IRepository;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.Common;
using Dominus.Domain.DTOs.AuthDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Dominus.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
      IGenericRepository<User> userRepository,
      IConfiguration configuration,
      IEmailService emailService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _emailService = emailService;
        }


        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequestDto)
        {
            var existingUser = await _userRepository.GetAsync(u => u.Email == registerRequestDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto(401, "User with this email already exists");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequestDto.Password);

            var user = new User
            {
                Name = registerRequestDto.Name,
                Email = registerRequestDto.Email,
                PasswordHash = passwordHash,
                Role = Roles.user,
                IsBlocked = false,
                CreatedOn = DateTime.UtcNow
            };

            //var admin = new User
            //{
            //    Name = "Admin",
            //    Email = "admin@gmail.com",
            //    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            //    Role = Roles.admin,
            //    IsBlocked = false,
            //    CreatedOn = DateTime.UtcNow
            //};

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(201, "User registered successfully");
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
        {

            var user = await _userRepository.GetAsync(u => u.Email == loginRequestDto.Email);
            if (user == null || user.IsDeleted)
            {
                return new AuthResponseDto(404, "User not found");
            }

            if (user.IsBlocked)
            {
                return new AuthResponseDto(403, "User account is blocked");
            }




            if (!BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.PasswordHash))
            {
                return new AuthResponseDto(401, "Invalid credentials");
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(200, "Login successful", accessToken, refreshToken);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetAsync(u => u.RefreshToken == refreshToken);
            if (user == null)
            {
                return new AuthResponseDto(401, "Invalid refresh token");
            }

            if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return new AuthResponseDto(401, "Refresh token has expired");
            }

            if (user.IsBlocked)
            {
                return new AuthResponseDto(403, "User account is blocked");
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(200, "Token refreshed successfully", newAccessToken, newRefreshToken);
        }
        public async Task<AuthResponseDto> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new AuthResponseDto(401, "Refresh token is required");
            }

            var user = await _userRepository.GetAsync(
                u => u.RefreshToken == refreshToken
            );

            if (user == null)
            {
                return new AuthResponseDto(401, "Invalid refresh token");
            }

            if (user.IsDeleted)
            {
                return new AuthResponseDto(404, "User not found");
            }

            if (user.IsBlocked)
            {
                return new AuthResponseDto(403, "User account is blocked");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(200, "Logout successful");
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
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),

                new Claim(ClaimTypes.Role, user.Role.ToString().ToLower())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
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


        public async Task<AuthResponseDto> GenerateAccessTokenFromRefreshAsync(string refreshToken)
        {
            var user = await _userRepository.GetAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.IsDeleted)
                return new AuthResponseDto(401, "Invalid refresh token");

            if (user.RefreshTokenExpiryTime == null ||
                user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return new AuthResponseDto(401, "Refresh token expired");

            if (user.IsBlocked)
                return new AuthResponseDto(403, "User account is blocked");

            var newAccessToken = GenerateJwtToken(user);

            return new AuthResponseDto(
                200,
                "Access token generated successfully",
                newAccessToken
            );
        }
        public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return new ApiResponse<UserProfileDto>(404, "User not found");
            if (user.IsBlocked)
                return new ApiResponse<UserProfileDto>(403, "User is blocked");
            var profile = new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsBlocked = user.IsBlocked,
                CreatedOn = user.CreatedOn
            };
            return new ApiResponse<UserProfileDto>(200, "Profile fetched successfully", profile
            );
        }


        public async Task<AuthResponseDto> UpdateProfileAsync(UpdateProfileRequestDto dto, int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || user.IsDeleted)
                return new AuthResponseDto(404, "User not found");

            if (user.IsBlocked)
                return new AuthResponseDto(403, "Blocked users cannot update profile");

            // ?? Email must match token email (cannot change)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var trimmedEmail = dto.Email.Trim();

                if (!trimmedEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                    return new AuthResponseDto(401, "Email cannot be changed. It must match your registered email");
            }

            bool updated = false;

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                user.Name = dto.Name.Trim();
                updated = true;
            }

            

            if (!updated)
                return new AuthResponseDto(401, "Nothing to update");

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(201, "Profile updated successfully");
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetAsync(u => u.Email == email);

            if (user == null || user.IsDeleted)
                return new AuthResponseDto(404, "User not found");

            if (user.IsBlocked)
                return new AuthResponseDto(403, "Blocked user cannot reset password");

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(3);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(200, "Password reset token generated", user.PasswordResetToken);
        }
        public async Task<AuthResponseDto> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _userRepository.GetAsync(
                u => u.PasswordResetToken == token
            );

            if (user == null)
                return new AuthResponseDto(401, "Invalid token");

            if (user.PasswordResetTokenExpiry == null)
                return new AuthResponseDto(401, "Token expired");

            // Force treat DB value as UTC
            var expiryUtc = DateTime.SpecifyKind(
                user.PasswordResetTokenExpiry.Value,
                DateTimeKind.Utc
            );

            if (expiryUtc <= DateTime.UtcNow)
                return new AuthResponseDto(401, "Token expired");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponseDto(201, "Password reset successfully");
        }

        public async Task<ApiResponse<string>> SendOtpAsync(string email)
        {
            var user = await _userRepository.GetAsync(u => u.Email == email);

            if (user == null || user.IsDeleted)
                return new ApiResponse<string>(404, "User not found");

            if (user.IsBlocked)
                return new ApiResponse<string>(403, "Blocked user cannot reset password");

            string otp = new Random().Next(100000, 999999).ToString();
            DateTime expiry = DateTime.UtcNow.AddMinutes(10);

            user.PasswordOtp = otp;
            user.PasswordOtpExpiry = expiry;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            string body = $@"
        <h2>Password Reset</h2>
        <p>Your OTP code is:</p>
        <h1 style='color:#2d89ef'>{otp}</h1>
        <p>Valid for 10 minutes.</p>
    ";

            await _emailService.SendEmailAsync(email, "Password Reset OTP", body);

            return new ApiResponse<string>(200, "OTP sent to email.");
        }
        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userRepository.GetAsync(u => u.Email == dto.Email);

            if (user == null)
                return new ApiResponse<string>(404, "User not found");

            if (string.IsNullOrEmpty(user.PasswordOtp))
                return new ApiResponse<string>(400, "No OTP generated");

            if (user.PasswordOtpExpiry == null || user.PasswordOtpExpiry <= DateTime.UtcNow)
                return new ApiResponse<string>(400, "OTP expired");

            if (user.PasswordOtp != dto.Otp)
                return new ApiResponse<string>(400, "Incorrect OTP");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            user.PasswordOtp = null;
            user.PasswordOtpExpiry = null;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new ApiResponse<string>(200, "Password reset successfully.");
        }


    }
}