using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static CategoryResponseDto ToDto(Category c) => new()
        {
            CategoryId = c.CategoryId,
            EventId = c.EventId,
            CategoryName = c.CategoryName,
            Distance = c.Distance,
            EntryFee = c.EntryFee,
        };

        // GET /api/events/{eventId}/categories  (public)
        [HttpGet("api/events/{eventId}/categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound();

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories.Select(ToDto));
        }

        // POST /api/events/{eventId}/categories  (Organiser, owner only)
        [HttpPost("api/events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<CategoryResponseDto>> Create(int eventId, CategoryCreateDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var category = new Category
            {
                EventId = eventId,
                CategoryName = dto.CategoryName,
                Distance = dto.Distance,
                EntryFee = dto.EntryFee,
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetForEvent), new { eventId }, ToDto(category));
        }

        // PUT /api/categories/{id}  (Organiser, owner only)
        [HttpPut("api/categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<CategoryResponseDto>> Update(int id, CategoryCreateDto dto)
        {
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            if (category.Event!.OrganiserId != CurrentUserId) return Forbid();

            category.CategoryName = dto.CategoryName;
            category.Distance = dto.Distance;
            category.EntryFee = dto.EntryFee;
            await _context.SaveChangesAsync();

            return Ok(ToDto(category));
        }

        // DELETE /api/categories/{id}  (Organiser, owner only)
        [HttpDelete("api/categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            if (category.Event!.OrganiserId != CurrentUserId) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}