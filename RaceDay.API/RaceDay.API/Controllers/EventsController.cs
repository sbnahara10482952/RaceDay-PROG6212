using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : BaseController
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            if (!IsLoggedIn())
                return Unauthorized();

            var events = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.EventCategories)
                .ThenInclude(ec => ec.Category)
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetEvent(int eventId)
        {
            if (!IsLoggedIn())
                return Unauthorized();

            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.EventCategories)
                .ThenInclude(ec => ec.Category)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            return Ok(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest request)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            if (request.DistanceKm <= 0)
                return BadRequest("Distance must be greater than zero.");

            if (!new[] { "Run", "Walk", "Cycle" }
                .Contains(request.EventType))
            {
                return BadRequest("Event type must be Run, Walk or Cycle.");
            }

            var newEvent = new Event
            {
                OrganiserID = CurrentUserId!.Value,
                EventName = request.EventName,
                Description = request.Description,
                EventDate = request.EventDate,
                StartTime = request.StartTime,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                Status = "Upcoming"
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { eventId = newEvent.EventID },
                newEvent);
        }

        [HttpPut("{eventId}")]
        public async Task<IActionResult> UpdateEvent(
            int eventId,
            CreateEventRequest request)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventItem = await _context.Events.FindAsync(eventId);

            if (eventItem == null)
                return NotFound();

            if (eventItem.OrganiserID != CurrentUserId)
                return Forbid();

            eventItem.EventName = request.EventName;
            eventItem.Description = request.Description;
            eventItem.EventDate = request.EventDate;
            eventItem.StartTime = request.StartTime;
            eventItem.Location = request.Location;
            eventItem.DistanceKm = request.DistanceKm;
            eventItem.EventType = request.EventType;

            await _context.SaveChangesAsync();

            return Ok(eventItem);
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventItem = await _context.Events.FindAsync(eventId);

            if (eventItem == null)
                return NotFound();

            if (eventItem.OrganiserID != CurrentUserId)
                return Forbid();

            eventItem.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event cancelled successfully."
            });
        }
    }

    public class CreateEventRequest
    {
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
    }
}