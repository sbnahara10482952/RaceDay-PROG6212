using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/categories")]
    public class CategoriesController : BaseController
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            if (!IsLoggedIn())
                return Unauthorized();

            var categories = await _context.EventCategories
                .Where(ec => ec.EventID == eventId)
                .Include(ec => ec.Category)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(
            int eventId,
            EventCategoryRequest request)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventItem = await _context.Events.FindAsync(eventId);

            if (eventItem == null)
                return NotFound("Event not found.");

            if (eventItem.OrganiserID != CurrentUserId)
                return Forbid();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == request.CategoryName);

            if (category == null)
            {
                category = new Category
                {
                    CategoryName = request.CategoryName,
                    DistanceKm = request.DistanceKm,
                    Description = request.Description
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            var eventCategory = new EventCategory
            {
                EventID = eventId,
                CategoryID = category.CategoryID,
                EntryFee = request.EntryFee,
                MaxParticipants = request.MaxParticipants,
                AvailableSlots = request.MaxParticipants
            };

            _context.EventCategories.Add(eventCategory);

            await _context.SaveChangesAsync();

            return Ok(eventCategory);
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(
            int eventId,
            int categoryId,
            EventCategoryRequest request)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventCategory = await _context.EventCategories
                .Include(ec => ec.Event)
                .Include(ec => ec.Category)
                .FirstOrDefaultAsync(ec =>
                    ec.EventID == eventId &&
                    ec.EventCategoryID == categoryId);

            if (eventCategory == null)
                return NotFound();

            if (eventCategory.Event!.OrganiserID != CurrentUserId)
                return Forbid();

            eventCategory.EntryFee = request.EntryFee;
            eventCategory.MaxParticipants = request.MaxParticipants;

            if (eventCategory.AvailableSlots > request.MaxParticipants)
                eventCategory.AvailableSlots = request.MaxParticipants;

            eventCategory.Category!.DistanceKm = request.DistanceKm;
            eventCategory.Category.Description = request.Description;

            await _context.SaveChangesAsync();

            return Ok(eventCategory);
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(
            int eventId,
            int categoryId)
        {
            if (!IsRole("Organiser"))
                return Forbid();

            var eventCategory = await _context.EventCategories
                .Include(ec => ec.Event)
                .FirstOrDefaultAsync(ec =>
                    ec.EventID == eventId &&
                    ec.EventCategoryID == categoryId);

            if (eventCategory == null)
                return NotFound();

            if (eventCategory.Event!.OrganiserID != CurrentUserId)
                return Forbid();

            _context.EventCategories.Remove(eventCategory);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event category removed."
            });
        }
    }

    public class EventCategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string? Description { get; set; }
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
    }
}