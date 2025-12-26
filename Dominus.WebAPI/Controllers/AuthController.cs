using Dominus.Application.DTOs.UserProfile;
using Dominus.Application.Interfaces.IServices;
using Dominus.Domain.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var response = await _authService.RegisterAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result.StatusCode != 200) return StatusCode(result.StatusCode, result);

            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(new
            {
                message = result.Message,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken
            });
        }


        [Authorize]
        [HttpGet("myProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "User not authenticated" });
            var userId = int.Parse(userIdClaim.Value);
            var response = await _authService.GetUserProfileAsync(userId);
            return StatusCode(response.StatusCode, response);
        }



        [Authorize]
        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
        {
            var userIdClaim = User.FindFirst("userId");

            if (userIdClaim == null)
                return Unauthorized(new { message = "User not authenticated" });

            var userId = int.Parse(userIdClaim.Value);

            var response = await _authService.UpdateProfileAsync(dto, userId);

            return StatusCode(response.StatusCode, response);
        }


       
        [Authorize]
        [HttpPost("Token/Refresh-Access")]
        public async Task<IActionResult> GetNewAccessToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new
                {
                    message = "Refresh token missing"
                });
            }

            var result = await _authService.GenerateAccessTokenFromRefreshAsync(refreshToken);

            if (result.StatusCode != 200)
                return StatusCode(result.StatusCode, result);

            SetTokenCookies(result.AccessToken, null);

            return Ok(new
            {
                message = result.Message,
                accessToken = result.AccessToken
            });
        }


        [Authorize]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            var emailFromToken = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(emailFromToken))
                return Unauthorized(new { message = "User not authenticated" });

            // 🔐 Prevent using another user's email
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.Trim() != emailFromToken)
                return BadRequest(new { message = "You can only request password reset for your own account" });

            var result = await _authService.ForgotPasswordAsync(emailFromToken);
            return StatusCode(result.StatusCode, result);
        }



        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> RefreshToken()
        //{
        //    var refreshToken = Request.Cookies["refreshToken"];
        //    if (string.IsNullOrEmpty(refreshToken))
        //        return BadRequest(new { message = "Refresh token missing" });

        //    var result = await _authService.RefreshTokenAsync(refreshToken);
        //    if (result.StatusCode != 200) return Unauthorized(result);

        //    SetTokenCookies(result.AccessToken, result.RefreshToken);

        //    return Ok(new
        //    {
        //        message = result.Message,
        //        accessToken = result.AccessToken,
        //        refreshToken = result.RefreshToken
        //    });
        //}

        //[HttpPost("revoke-token")]
        //public async Task<IActionResult> RevokeToken()
        //{
        //    var refreshToken = Request.Cookies["refreshToken"];
        //    if (string.IsNullOrEmpty(refreshToken))
        //        return BadRequest(new { message = "Refresh token missing" });

        //    var success = await _authService.RevokeTokenAsync(refreshToken);
        //    if (!success) return BadRequest(new { message = "Invalid token" });

        //    DeleteTokenCookies();
        //    return Ok(new { message = "Token revoked successfully" });
        //}

        private void SetTokenCookies(string? accessToken, string? refreshToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                Response.Cookies.Append("accessToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(1)
                });
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
            }
        }

        private void DeleteTokenCookies()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
        }

        //private void DeleteTokenCookies()
        //{
        //    Response.Cookies.Delete("accessToken");
        //    Response.Cookies.Delete("refreshToken");
        //}


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new
                {
                    message = "Refresh token missing"
                });
            }

            var response = await _authService.LogoutAsync(refreshToken);

            if (response.StatusCode != 200)
                return StatusCode(response.StatusCode, response);

            DeleteTokenCookies();

            return Ok(new
            {
                message = response.Message
            });

        }


    }
}












