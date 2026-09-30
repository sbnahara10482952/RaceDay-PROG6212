using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class ResultsController : BaseController
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("/api/enrolments/{enrolmentId}/result")]
        public async Task<IActionResult> CaptureResult(
            int enrolmentId,
            ResultRequest request)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var enrolment = await _context.Enrolments
                .Include(e => e.EventCategory)
                .ThenInclude(ec => ec!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            if (enrolment.EventCategory!.Event!.OrganiserID != CurrentUserId)
                return Forbid();

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentID == enrolmentId);

            if (result == null)
            {
                result = new Result
                {
                    EnrolmentID = enrolmentId
                };

                _context.Results.Add(result);
            }

            result.FinishTime = request.FinishTime;
            result.Position = request.Position;
            result.ResultStatus = request.ResultStatus;

            enrolment.EnrolmentStatus = "Completed";

            await _context.SaveChangesAsync();

            return Ok(result);
        }

        [HttpGet]
        [Route("/api/results/me")]
        public async Task<IActionResult> GetMyResults()
        {
            if (!IsRole("Participant"))
                return Forbid();

            var results = await _context.Results
                .Where(r => r.Enrolment!.ParticipantID == CurrentUserId)
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.EventCategory)
                .ThenInclude(ec => ec!.Event)
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet]
        [Route("/api/events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventItem = await _context.Events.FindAsync(eventId);

            if (eventItem == null)
                return NotFound();

            if (eventItem.OrganiserID != CurrentUserId)
                return Forbid();

            var results = await _context.Results
                .Where(r =>
                    r.Enrolment!.EventCategory!.EventID == eventId)
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Participant)
                .ToListAsync();

            return Ok(results);
        }
    }

    public class ResultRequest
    {
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string ResultStatus { get; set; } = "Finished";
    }
}