using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : BaseController
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public EventsController(
            RaceDayDbContext context,
            BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
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

        [HttpPost("{eventId}/banner")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadBanner(
            int eventId,
            IFormFile file)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserID != CurrentUserId)
                return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest("Please select an image.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Image must not exceed 5 MB.");

            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest("Only JPEG, PNG and WebP images are allowed.");

            string blobName;

            using (var stream = file.OpenReadStream())
            {
                blobName = await _blobStorageService.UploadImageAsync(
                    stream,
                    file.ContentType.ToLowerInvariant());
            }

            eventItem.BannerImageUrl = blobName;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event banner uploaded successfully.",
                imageName = blobName
            });
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

        [HttpGet("{eventId}/banner")]
        public async Task<IActionResult> GetBanner(int eventId)
        {
            if (!IsLoggedIn())
                return Unauthorized();

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (string.IsNullOrWhiteSpace(eventItem.BannerImageUrl))
                return NotFound("This event has no banner image.");

            var image = await _blobStorageService.DownloadImageAsync(
                eventItem.BannerImageUrl);

            if (image == null)
                return NotFound("Banner image not found.");

            return File(
                image.Content.ToStream(),
                image.Details.ContentType ?? "application/octet-stream");
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