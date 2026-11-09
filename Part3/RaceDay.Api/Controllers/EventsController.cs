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
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IBlobStorageService _blobs;

        public EventsController(RaceDayContext context, IBlobStorageService blobs)
        {
            _context = context;
            _blobs = blobs;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static EventResponseDto ToDto(Event ev, int enrolmentCount = 0) => new()
        {
            EventId = ev.EventId,
            OrganiserId = ev.OrganiserId,
            EventName = ev.EventName,
            EventDate = ev.EventDate,
            Location = ev.Location,
            Description = ev.Description,
            Distance = ev.Distance,
            EventType = ev.EventType,
            BannerUrl = ev.BannerUrl,
            EnrolmentCount = enrolmentCount,
        };

        // GET /api/events?type=Run&search=city&upcomingOnly=true  (public)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAll(
            [FromQuery] string? type, [FromQuery] string? search, [FromQuery] bool upcomingOnly = false)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(e => e.EventType == type);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.EventName.Contains(search) || e.Location.Contains(search));
            }

            if (upcomingOnly)
            {
                var today = DateTime.UtcNow.Date;
                query = query.Where(e => e.EventDate >= today);
            }

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();
            return Ok(events.Select(e => ToDto(e)));
        }

        // GET /api/events/mine  (Organiser: own events with enrolment counts)
        [HttpGet("mine")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetMine()
        {
            var userId = CurrentUserId;

            var events = await _context.Events
                .Where(e => e.OrganiserId == userId)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            var eventIds = events.Select(e => e.EventId).ToList();

            var counts = await _context.Categories
                .Where(c => eventIds.Contains(c.EventId))
                .Select(c => new { c.EventId, Count = c.Enrolments.Count(en => en.Status != "Cancelled") })
                .ToListAsync();

            return Ok(events.Select(e =>
                ToDto(e, counts.Where(c => c.EventId == e.EventId).Sum(c => c.Count))));
        }

        // GET /api/events/{id}  (public)
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventResponseDto>> GetById(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            return Ok(ToDto(ev));
        }

        // POST /api/events  (Organiser only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<EventResponseDto>> Create(EventCreateDto dto)
        {
            var ev = new Event
            {
                OrganiserId = CurrentUserId,
                EventName = dto.EventName,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Description = dto.Description,
                Distance = dto.Distance,
                EventType = dto.EventType,
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ToDto(ev));
        }

        // PUT /api/events/{id}  (Organiser, owner only)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<EventResponseDto>> Update(int id, EventCreateDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            ev.EventName = dto.EventName;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.Description = dto.Description;
            ev.Distance = dto.Distance;
            ev.EventType = dto.EventType;

            await _context.SaveChangesAsync();
            return Ok(ToDto(ev));
        }

        // DELETE /api/events/{id}  (Organiser, owner only)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST /api/events/{id}/banner  (Organiser, owner only; stored in Azure Blob Storage)
        [HttpPost("{id:int}/banner")]
        [Authorize(Roles = "Organiser")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<EventResponseDto>> UploadBanner(int id, IFormFile file)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var error = ImageRules.Check(file);
            if (error != null) return BadRequest(error);

            ev.BannerUrl = await _blobs.UploadImageAsync(file, "banners");
            await _context.SaveChangesAsync();

            return Ok(ToDto(ev));
        }

        // GET /api/events/{eventId}/enrolments  (Organiser, owner only)
        [HttpGet("{eventId:int}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetEnrolmentsForEvent(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var enrolments = await _context.EventEnrolments
                .Include(en => en.Participant)
                .Include(en => en.Category)
                .Include(en => en.Result)
                .Where(en => en.Category!.EventId == eventId)
                .OrderBy(en => en.EnrolmentDate)
                .ToListAsync();

            var result = enrolments.Select(en => new EnrolmentResponseDto
            {
                EnrolmentId = en.EnrolmentId,
                ParticipantId = en.ParticipantId,
                ParticipantName = en.Participant?.Name ?? string.Empty,
                CategoryId = en.CategoryId,
                CategoryName = en.Category!.CategoryName,
                EventId = eventId,
                EventName = ev.EventName,
                EventDate = ev.EventDate,
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
            });

            return Ok(result);
        }
    }
}