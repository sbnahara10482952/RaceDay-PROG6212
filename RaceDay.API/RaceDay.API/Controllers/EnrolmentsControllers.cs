using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    public class EnrolmentsController : BaseController
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            CreateEnrolmentRequest request)
        {
            if (!IsRole("Participant"))
                return Forbid();

            var eventCategory = await _context.EventCategories
                .Include(ec => ec.Event)
                .FirstOrDefaultAsync(ec =>
                    ec.EventCategoryID == request.EventCategoryID);

            if (eventCategory == null)
                return NotFound("Event category not found.");

            if (eventCategory.AvailableSlots <= 0)
                return BadRequest("No available slots.");

            var existing = await _context.Enrolments.AnyAsync(e =>
                e.ParticipantID == CurrentUserId &&
                e.EventCategoryID == request.EventCategoryID &&
                e.EnrolmentStatus != "Cancelled");

            if (existing)
                return Conflict("You are already enrolled.");

            var enrolment = new Enrolment
            {
                ParticipantID = CurrentUserId!.Value,
                EventCategoryID = request.EventCategoryID,
                EnrolmentStatus = "Confirmed",
                RaceNumber = request.RaceNumber
            };

            eventCategory.AvailableSlots--;

            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            return Ok(enrolment);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!IsRole("Participant"))
                return Forbid();

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantID == CurrentUserId)
                .Include(e => e.EventCategory)
                .ThenInclude(ec => ec!.Event)
                .Include(e => e.EventCategory)
                .ThenInclude(ec => ec!.Category)
                .Include(e => e.Result)
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpGet]
        [Route("/api/events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventItem = await _context.Events.FindAsync(eventId);

            if (eventItem == null)
                return NotFound();

            if (eventItem.OrganiserID != CurrentUserId)
                return Forbid();

            var enrolments = await _context.Enrolments
                .Where(e => e.EventCategory!.EventID == eventId)
                .Include(e => e.Participant)
                .Include(e => e.EventCategory)
                .ThenInclude(ec => ec!.Category)
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpPut("{enrolmentId}/status")]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int enrolmentId,
            UpdateEnrolmentStatusRequest request)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            if (request.EnrolmentStatus != "Confirmed" &&
                request.EnrolmentStatus != "Cancelled")
            {
                return BadRequest("Status must be Confirmed or Cancelled.");
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.EventCategory)
                .ThenInclude(ec => ec!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.EventCategory!.Event!.OrganiserID != CurrentUserId)
                return Forbid();

            if (enrolment.EnrolmentStatus == "Completed")
                return BadRequest("Completed enrolments cannot be changed.");

            if (enrolment.EnrolmentStatus == request.EnrolmentStatus)
                return BadRequest("The enrolment already has this status.");

            if (request.EnrolmentStatus == "Confirmed")
            {
                if (enrolment.EventCategory.AvailableSlots <= 0)
                    return BadRequest("There are no available slots.");

                enrolment.EventCategory.AvailableSlots--;
            }
            else if (enrolment.EnrolmentStatus == "Confirmed")
            {
                enrolment.EventCategory.AvailableSlots++;
            }

            enrolment.EnrolmentStatus = request.EnrolmentStatus;

            await _context.SaveChangesAsync();

            return Ok(enrolment);
        }
    }

    public class CreateEnrolmentRequest
    {
        public int EventCategoryID { get; set; }
        public int RaceNumber { get; set; }
    }

    public class UpdateEnrolmentStatusRequest
    {
        public string EnrolmentStatus { get; set; } = string.Empty;
    }
}