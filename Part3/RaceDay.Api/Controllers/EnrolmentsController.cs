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

        private static EnrolmentResponseDto ToDto(EventEnrolment en, int? totalFinishers = null) => new()
        {
            EnrolmentId = en.EnrolmentId,
            ParticipantId = en.ParticipantId,
            ParticipantName = en.Participant?.Name ?? string.Empty,
            CategoryId = en.CategoryId,
            CategoryName = en.Category?.CategoryName ?? string.Empty,
            EventId = en.Category?.EventId ?? 0,
            EventName = en.Category?.Event?.EventName ?? string.Empty,
            EventDate = en.Category?.Event?.EventDate ?? default,
            EnrolmentDate = en.EnrolmentDate,
            Status = en.Status,
            TotalFinishers = totalFinishers,
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
        [HttpPost("api/categories/{categoryId:int}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<ActionResult<EnrolmentResponseDto>> Enrol(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null) return NotFound();

            var userId = CurrentUserId;
            var existing = await _context.EventEnrolments
                .FirstOrDefaultAsync(en => en.ParticipantId == userId && en.CategoryId == categoryId);

            if (existing != null && existing.Status != "Cancelled")
            {
                return Conflict("Already enrolled in this category.");
            }

            EventEnrolment enrolment;
            if (existing != null)
            {
                // Entering again after cancelling: reactivate the same record.
                existing.Status = "Pending";
                existing.EnrolmentDate = DateTime.UtcNow;
                enrolment = existing;
            }
            else
            {
                enrolment = new EventEnrolment
                {
                    ParticipantId = userId,
                    CategoryId = categoryId,
                    EnrolmentDate = DateTime.UtcNow,
                    Status = "Pending",
                };
                _context.EventEnrolments.Add(enrolment);
            }

            await _context.SaveChangesAsync();

            var saved = await _context.EventEnrolments
                .Include(en => en.Participant)
                .Include(en => en.Category).ThenInclude(c => c!.Event)
                .FirstAsync(en => en.EnrolmentId == enrolment.EnrolmentId);

            return CreatedAtAction(nameof(GetForUser), new { id = userId }, ToDto(saved));
        }

        // GET /api/users/{id}/enrolments  (own enrolments only, with results and finisher counts)
        [HttpGet("api/users/{id:int}/enrolments")]
        public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetForUser(int id)
        {
            if (id != CurrentUserId) return Forbid();

            var enrolments = await _context.EventEnrolments
                .Include(en => en.Participant)
                .Include(en => en.Category).ThenInclude(c => c!.Event)
                .Include(en => en.Result)
                .Where(en => en.ParticipantId == id)
                .OrderByDescending(en => en.Category!.Event!.EventDate)
                .ToListAsync();

            // How many people finished each event (for "47th of 312").
            var eventIds = enrolments.Select(en => en.Category!.EventId).Distinct().ToList();

            var finisherEventIds = await _context.Results
                .Where(r => r.Status == "Finished" && eventIds.Contains(r.Enrolment!.Category!.EventId))
                .Select(r => r.Enrolment!.Category!.EventId)
                .ToListAsync();

            var finisherCounts = finisherEventIds
                .GroupBy(eventId => eventId)
                .ToDictionary(g => g.Key, g => g.Count());

            return Ok(enrolments.Select(en =>
            {
                int? total = en.Result != null
                    ? finisherCounts.GetValueOrDefault(en.Category!.EventId)
                    : (int?)null;
                return ToDto(en, total);
            }));
        }

        // PUT /api/enrolments/{id}/confirm  (Organiser, owner of the event)
        [HttpPut("api/enrolments/{id:int}/confirm")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Confirm(int id)
        {
            var enrolment = await _context.EventEnrolments
                .Include(en => en.Category).ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(en => en.EnrolmentId == id);

            if (enrolment == null) return NotFound();
            if (enrolment.Category!.Event!.OrganiserId != CurrentUserId) return Forbid();
            if (enrolment.Status == "Cancelled") return Conflict("A cancelled enrolment cannot be confirmed.");

            enrolment.Status = "Confirmed";
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE /api/enrolments/{id}  (Participant, owner only)
        [HttpDelete("api/enrolments/{id:int}")]
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