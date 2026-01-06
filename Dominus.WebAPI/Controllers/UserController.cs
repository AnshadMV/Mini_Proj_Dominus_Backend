using Dominus.Application.DTOs.UserProfile;
using Dominus.Application.Interfaces.IServices;
using Dominus.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dominus.WebAPI.Controllers
{
    [Route("api/[controller]/Admin")]
    [ApiController]
    [Authorize(Roles = "admin")] 
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UsersController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }



        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsersAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("GetBy_{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("Toggle/Block_Unblock")]
        public async Task<IActionResult> BlockUnblock(int id)
        {
            var response = await _userService.BlockUnblockUserAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("DeleteBy_{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var response = await _userService.SoftDeleteUserAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
