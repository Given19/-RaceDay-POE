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
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static EnrolmentResponseDto ToDto(EventEnrolment en) => new()
        {
            EnrolmentId = en.EnrolmentId,
            ParticipantId = en.ParticipantId,
            CategoryId = en.CategoryId,
            CategoryName = en.Category?.CategoryName ?? string.Empty,
            EventId = en.Category?.EventId ?? 0,
            EventName = en.Category?.Event?.EventName ?? string.Empty,
            EnrolmentDate = en.EnrolmentDate,
            Status = en.Status,
            Result = en.Result == null ? null : new ResultResponseDto
            {
                ResultId = en.Result.ResultId,
                EnrolmentId = en.Result.EnrolmentId,
                FinishTime = en.Result.FinishTime,
                Position = en.Result.Position,
                Status = en.Result.Status,
            },
        };

        // POST /api/categories/{categoryId}/enrolments  (Participant only)
        [HttpPost("api/categories/{categoryId}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<EnrolmentResponseDto>> Enrol(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null) return NotFound();

            bool alreadyEnrolled = await _context.EventEnrolments
                .AnyAsync(en => en.ParticipantId == CurrentUserId && en.CategoryId == categoryId);
            if (alreadyEnrolled) return Conflict("Already enrolled in this category.");

            var enrolment = new EventEnrolment
            {
                ParticipantId = CurrentUserId,
                CategoryId = categoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed",
            };

            _context.EventEnrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            var saved = await _context.EventEnrolments
                .Include(en => en.Category).ThenInclude(c => c!.Event)
                .FirstAsync(en => en.EnrolmentId == enrolment.EnrolmentId);

            return CreatedAtAction(nameof(GetForUser), new { id = CurrentUserId }, ToDto(saved));
        }

        // GET /api/users/{id}/enrolments  (self only)
        [HttpGet("api/users/{id}/enrolments")]
        public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetForUser(int id)
        {
            if (id != CurrentUserId) return Forbid();

            var enrolments = await _context.EventEnrolments
                .Include(en => en.Category).ThenInclude(c => c!.Event)
                .Include(en => en.Result)
                .Where(en => en.ParticipantId == id)
                .ToListAsync();

            return Ok(enrolments.Select(ToDto));
        }

        // DELETE /api/enrolments/{id}  (Participant, owner only)
        [HttpDelete("api/enrolments/{id}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Cancel(int id)
        {
            var enrolment = await _context.EventEnrolments.FindAsync(id);
            if (enrolment == null) return NotFound();
            if (enrolment.ParticipantId != CurrentUserId) return Forbid();

            enrolment.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}