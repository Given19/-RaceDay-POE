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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // POST /api/enrolments/{enrolmentId}/result  (Organiser, must own the event)
        [HttpPost("api/enrolments/{enrolmentId}/result")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<ResultResponseDto>> Capture(int enrolmentId, ResultCreateDto dto)
        {
            var enrolment = await _context.EventEnrolments
                .Include(en => en.Category).ThenInclude(c => c!.Event)
                .Include(en => en.Result)
                .FirstOrDefaultAsync(en => en.EnrolmentId == enrolmentId);

            if (enrolment == null) return NotFound();

            var ownerId = enrolment.Category!.Event!.OrganiserId;
            if (ownerId != CurrentUserId) return Forbid();

            if (enrolment.Result != null)
            {
                return Conflict("A result has already been captured for this enrolment.");
            }

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                Status = dto.Status,
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetForEvent), new { eventId = enrolment.Category.EventId },
                new ResultResponseDto
                {
                    ResultId = result.ResultId,
                    EnrolmentId = result.EnrolmentId,
                    FinishTime = result.FinishTime,
                    Position = result.Position,
                    Status = result.Status,
                });
        }

        // GET /api/events/{eventId}/results  (public)
        [HttpGet("api/events/{eventId}/results")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ResultResponseDto>>> GetForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound();

            var results = await _context.Results
                .Include(r => r.Enrolment).ThenInclude(en => en!.Category)
                .Where(r => r.Enrolment!.Category!.EventId == eventId)
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results.Select(r => new ResultResponseDto
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                FinishTime = r.FinishTime,
                Position = r.Position,
                Status = r.Status,
            }));
        }
    }
}