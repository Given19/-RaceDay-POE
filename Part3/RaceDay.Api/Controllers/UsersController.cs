using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public UsersController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetProfile(int id)
        {
            if (id != CurrentUserId)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
            });
        }

        // PUT /api/users/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateProfile(int id, UpdateProfileDto dto)
        {
            if (id != CurrentUserId)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Name = dto.Name;
            user.Email = dto.Email;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
            });
        }
    }
}