using Dominus.Domain.DTOs.ColorDTOs;
using Dominus.Domain.Entities;
using Dominus.Domain.Interfaces;
using Dominus.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dominus.WebAPI.Controllers
{
    [ApiController]
    [Route("api/colors")]
    
    public class ColorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ColorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Colors.Where(c => c.IsActive).ToListAsync());
        }


        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create(string name, string hexCode)
        {
            var color = new Color { Name = name, HexCode = hexCode };
            _context.Colors.Add(color);
            await _context.SaveChangesAsync();
            return Ok(color);
        }

       
    }


}
