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
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private static EventResponseDto ToDto(Event ev) => new()
        {
            EventId = ev.EventId,
            OrganiserId = ev.OrganiserId,
            EventName = ev.EventName,
            EventDate = ev.EventDate,
            Location = ev.Location,
            Description = ev.Description,
            Distance = ev.Distance,
            EventType = ev.EventType,
        };

        // GET /api/events  (public)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAll()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events.Select(ToDto));
        }

        // GET /api/events/{id}  (public)
        [HttpGet("{id}")]
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
        [HttpPut("{id}")]
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
        [HttpDelete("{id}")]
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

        // GET /api/events/{eventId}/enrolments  (Organiser, owner only)
        [HttpGet("{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetEnrolmentsForEvent(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var enrolments = await _context.EventEnrolments
                .Include(en => en.Category)
                .Include(en => en.Result)
                .Where(en => en.Category!.EventId == eventId)
                .ToListAsync();

            var result = enrolments.Select(en => new EnrolmentResponseDto
            {
                EnrolmentId = en.EnrolmentId,
                ParticipantId = en.ParticipantId,
                CategoryId = en.CategoryId,
                CategoryName = en.Category!.CategoryName,
                EventId = eventId,
                EventName = ev.EventName,
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