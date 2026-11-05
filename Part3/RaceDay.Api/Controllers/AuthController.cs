using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private static readonly string[] ValidRoles = { "Organiser", "Participant" };
        private readonly RaceDayContext _context;

        public AuthController(RaceDayContext context)
        {
            _context = context;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterDto dto)
        {
            if (!ValidRoles.Contains(dto.Role))
            {
                return BadRequest($"Role must be one of: {string.Join(", ", ValidRoles)}");
            }

            bool emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailTaken)
            {
                return Conflict("A user with this email is already registered.");
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                CreatedDate = DateTime.UtcNow,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
            };

            return CreatedAtAction(nameof(Register), new { id = user.UserId }, response);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            // Server-side session: UserId and Role are stored in the session
            // store, keyed by a session ID in the cookie - not in the cookie itself.
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Name", user.Name);

            return Ok(new UserResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
            });
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok();
        }
    }
}