using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IBlobStorageService _blobs;

        public UsersController(RaceDayContext context, IBlobStorageService blobs)
        {
            _context = context;
            _blobs = blobs;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static UserResponseDto ToDto(User user) => new()
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl,
        };

        // GET /api/users/{id}  (own profile only)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetProfile(int id)
        {
            if (id != CurrentUserId) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return Ok(ToDto(user));
        }

        // PUT /api/users/{id}  (own profile only)
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> UpdateProfile(int id, UpdateProfileDto dto)
        {
            if (id != CurrentUserId) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            bool emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != id);
            if (emailTaken) return Conflict("That email is already used by another account.");

            user.Name = dto.Name;
            user.Email = dto.Email;
            await _context.SaveChangesAsync();

            return Ok(ToDto(user));
        }

        // POST /api/users/{id}/profile-picture  (own profile only)
        [HttpPost("{id:int}/profile-picture")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<UserResponseDto>> UploadProfilePicture(int id, IFormFile file)
        {
            if (id != CurrentUserId) return Forbid();

            var error = ImageRules.Check(file);
            if (error != null) return BadRequest(error);

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.ProfilePictureUrl = await _blobs.UploadImageAsync(file, "profiles");
            await _context.SaveChangesAsync();

            return Ok(ToDto(user));
        }
    }
}