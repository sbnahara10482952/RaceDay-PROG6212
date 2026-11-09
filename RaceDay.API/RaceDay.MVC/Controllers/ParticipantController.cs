using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ParticipantController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Login", "Account");
            }

            var result = await _apiService.GetAsync("api/events");

            if (!result.Success || result.Data == null)
            {
                ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(result.Message)
                    ? "Unable to load upcoming events."
                    : result.Message;

                return View(new List<EventViewModel>());
            }

            var events = new List<EventViewModel>();

            if (result.Data.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in result.Data.Value.EnumerateArray())
                {
                    var eventDate = item.GetProperty("eventDate").GetDateTime();

                    var status = item.TryGetProperty("status", out var statusValue)
                        ? statusValue.GetString() ?? ""
                        : "";

                    bool isUpcomingStatus =
                        status.Equals("Upcoming", StringComparison.OrdinalIgnoreCase) ||
                        status.Equals("Open", StringComparison.OrdinalIgnoreCase);

                    if (eventDate.Date < DateTime.Today || !isUpcomingStatus)
                    {
                        continue;
                    }

                    events.Add(new EventViewModel
                    {
                        EventID = item.GetProperty("eventID").GetInt32(),
                        EventName = item.GetProperty("eventName").GetString() ?? "",
                        Description = item.TryGetProperty("description", out var description)
                            ? description.GetString()
                            : null,
                        EventDate = eventDate,
                        StartTime = item.TryGetProperty("startTime", out var startTime)
                            ? TimeSpan.Parse(startTime.GetString() ?? "00:00:00")
                            : TimeSpan.Zero,
                        Location = item.GetProperty("location").GetString() ?? "",
                        DistanceKm = item.GetProperty("distanceKm").GetDecimal(),
                        EventType = item.GetProperty("eventType").GetString() ?? "",
                        Status = status
                    });
                }
            }

            return View(events.OrderBy(e => e.EventDate).ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Login", "Account");
            }

            var result = await _apiService.GetAsync($"api/events/{id}");

            if (!result.Success || result.Data == null)
            {
                return NotFound("Event details could not be found.");
            }

            var item = result.Data.Value;

            if (item.ValueKind != JsonValueKind.Object)
            {
                return NotFound();
            }

            var eventDate = item.GetProperty("eventDate").GetDateTime();

            var eventViewModel = new EventViewModel
            {
                EventID = item.GetProperty("eventID").GetInt32(),
                EventName = item.GetProperty("eventName").GetString() ?? "",
                Description = item.TryGetProperty("description", out var description)
                    ? description.GetString()
                    : null,
                EventDate = eventDate,
                StartTime = item.TryGetProperty("startTime", out var startTime)
                    ? TimeSpan.Parse(startTime.GetString() ?? "00:00:00")
                    : TimeSpan.Zero,
                Location = item.GetProperty("location").GetString() ?? "",
                DistanceKm = item.GetProperty("distanceKm").GetDecimal(),
                EventType = item.GetProperty("eventType").GetString() ?? "",
                Status = item.GetProperty("status").GetString() ?? "",

                BannerImageUrl = item.TryGetProperty("bannerImageUrl", out var banner)
                    && banner.ValueKind != JsonValueKind.Null
                        ? banner.GetString()
                        : null
            };

            // Retrieve available categories for this event.
            var categoriesResult = await _apiService.GetAsync(
                $"api/events/{id}/categories");

            if (categoriesResult.Success &&
                categoriesResult.Data.HasValue &&
                categoriesResult.Data.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var category in categoriesResult.Data.Value.EnumerateArray())
                {
                    var categoryDetails = category.GetProperty("category");

                    eventViewModel.Categories.Add(new EventCategoryViewModel
                    {
                        EventCategoryID = category.GetProperty("eventCategoryID").GetInt32(),

                        EventID = category.GetProperty("eventID").GetInt32(),

                        CategoryID = category.GetProperty("categoryID").GetInt32(),

                        CategoryName = categoryDetails.GetProperty("categoryName").GetString() ?? "",

                        DistanceKm = categoryDetails.GetProperty("distanceKm").GetDecimal(),

                        Description = categoryDetails.TryGetProperty("description", out var categoryDescription)
                            ? categoryDescription.GetString()
                            : null,

                        EntryFee = category.GetProperty("entryFee").GetDecimal(),

                        MaxParticipants = category.GetProperty("maxParticipants").GetInt32(),

                        AvailableSlots = category.GetProperty("availableSlots").GetInt32()
                    });
                }
            }

            return View(eventViewModel);
        }

        // GET: Retrieve private event banner
        [HttpGet]
        public async Task<IActionResult> EventBanner(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || role != "Participant")
            {
                return Unauthorized();
            }

            var result = await _apiService.GetImageAsync(
                $"api/events/{id}/banner");

            if (!result.Success || result.ImageData == null)
            {
                return NotFound();
            }

            return File(
                result.ImageData,
                result.ContentType ?? "image/jpeg");
        }

        // GET: Participant Results
        public async Task<IActionResult> MyResults()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Login", "Account");
            }

            var response = await _apiService.GetAsync("api/results/me");

            if (!response.Success || response.Data == null)
            {
                ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Unable to load your race results."
                    : response.Message;

                return View(new List<ResultViewModel>());
            }

            var results = new List<ResultViewModel>();

            if (response.Data.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in response.Data.Value.EnumerateArray())
                {
                    var enrolment = item.GetProperty("enrolment");
                    var eventCategory = enrolment.GetProperty("eventCategory");
                    var eventData = eventCategory.GetProperty("event");

                    var model = new ResultViewModel
                    {
                        EnrolmentID = enrolment.GetProperty("enrolmentID").GetInt32(),

                        ParticipantID = enrolment.GetProperty("participantID").GetInt32(),

                        RaceNumber = enrolment.GetProperty("raceNumber").GetInt32(),

                        EventName = eventData.GetProperty("eventName").GetString() ?? "",

                        EventDate = eventData.GetProperty("eventDate").GetDateTime(),

                        Location = eventData.GetProperty("location").GetString() ?? "",

                        FinishTime = item.TryGetProperty("finishTime", out var finishTime)
                            && finishTime.ValueKind != JsonValueKind.Null
                                ? finishTime.GetString() ?? ""
                                : "",

                        Position = item.TryGetProperty("position", out var position)
                            && position.ValueKind != JsonValueKind.Null
                                ? position.GetInt32()
                                : null,

                        ResultStatus = item.GetProperty("resultStatus").GetString() ?? ""
                    };

                    results.Add(model);
                }
            }

            return View(results.OrderBy(r => r.EventDate).ToList());
        }

        // GET: Participant Profile
        public async Task<IActionResult> Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Login", "Account");
            }

            var response = await _apiService.GetAsync("api/users/me");

            if (!response.Success || response.Data == null)
            {
                ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Unable to load your profile."
                    : response.Message;

                return View(new ProfileViewModel());
            }

            var item = response.Data.Value;

            var model = new ProfileViewModel
            {
                UserID = item.GetProperty("userID").GetInt32(),
                FirstName = item.GetProperty("firstName").GetString() ?? "",
                LastName = item.GetProperty("lastName").GetString() ?? "",
                Email = item.GetProperty("email").GetString() ?? "",
                PhoneNumber = item.TryGetProperty("phoneNumber", out var phone)
                    && phone.ValueKind != JsonValueKind.Null
                        ? phone.GetString()
                        : "",
                Role = item.GetProperty("role").GetString() ?? "",
                DateCreated = item.GetProperty("dateCreated").GetDateTime()
            };

            return View(model);
        }

        // POST: Update Participant Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.PutAsync(
                "api/users/me",
                new
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber
                });

            if (!response.Success)
            {
                ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Unable to update your profile."
                    : response.Message;

                return View(model);
            }

            TempData["SuccessMessage"] = "Your profile has been updated successfully.";

            return RedirectToAction("Profile");
        }

        // POST: Participant Enrolment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterEvent(
            int eventId,
            int eventCategoryId)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve categories to verify the selected category.
            var categoriesResult = await _apiService.GetAsync(
                $"api/events/{eventId}/categories");

            if (!categoriesResult.Success || categoriesResult.Data == null)
            {
                TempData["ErrorMessage"] = "Unable to verify the selected race category.";
                return RedirectToAction("Details", new { id = eventId });
            }

            var categories = categoriesResult.Data.Value;

            var selectedCategory = categories.EnumerateArray()
                .FirstOrDefault(c =>
                    c.GetProperty("eventCategoryID").GetInt32() == eventCategoryId);

            if (selectedCategory.ValueKind == JsonValueKind.Undefined)
            {
                TempData["ErrorMessage"] = "The selected race category is invalid.";
                return RedirectToAction("Details", new { id = eventId });
            }

            int availableSlots = selectedCategory
                .GetProperty("availableSlots").GetInt32();

            if (availableSlots <= 0)
            {
                TempData["ErrorMessage"] = "This race category is fully booked.";
                return RedirectToAction("Details", new { id = eventId });
            }

            // Generate a race number.
            int raceNumber = Random.Shared.Next(10000, 99999);

            // Submit the enrolment to the API.
            var result = await _apiService.PostAsync(
                "api/enrolments",
                new
                {
                    EventCategoryID = eventCategoryId,
                    RaceNumber = raceNumber
                });

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Details", new { id = eventId });
            }

            TempData["SuccessMessage"] =
                $"You have successfully entered this event. Your race number is {raceNumber}.";

            return RedirectToAction("Details", new { id = eventId });
        }

        // GET: Participant My Events
        public async Task<IActionResult> MyEvents()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Login", "Account");
            }

            var response = await _apiService.GetAsync("api/enrolments/me");

            if (!response.Success || response.Data == null)
            {
                ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Unable to load your event enrolments."
                    : response.Message;

                return View(new List<EnrolmentViewModel>());
            }

            var enrolments = new List<EnrolmentViewModel>();

            if (response.Data.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in response.Data.Value.EnumerateArray())
                {
                    if (!item.TryGetProperty("eventCategory", out var eventCategory))
                    {
                        continue;
                    }

                    if (!eventCategory.TryGetProperty("event", out var eventData))
                    {
                        continue;
                    }

                    if (!eventCategory.TryGetProperty("category", out var category))
                    {
                        continue;
                    }

                    var enrolment = new EnrolmentViewModel
                    {
                        EnrolmentID = item.GetProperty("enrolmentID").GetInt32(),

                        ParticipantID = item.GetProperty("participantID").GetInt32(),

                        EventName = eventData.GetProperty("eventName").GetString() ?? "",

                        EventDate = eventData.GetProperty("eventDate").GetDateTime(),

                        Location = eventData.GetProperty("location").GetString() ?? "",

                        CategoryName = category.GetProperty("categoryName").GetString() ?? "",

                        DistanceKm = category.GetProperty("distanceKm").GetDecimal(),

                        RaceNumber = item.GetProperty("raceNumber").GetInt32(),

                        EnrolmentStatus = item.GetProperty("enrolmentStatus").GetString() ?? "",

                        EnrolmentDate = item.GetProperty("enrolmentDate").GetDateTime()
                    };

                    enrolments.Add(enrolment);
                }
            }

            return View(enrolments.OrderBy(e => e.EventDate).ToList());
        }
    }
}