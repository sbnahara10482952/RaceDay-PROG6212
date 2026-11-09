using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public OrganiserController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Organiser")
            {
                return RedirectToAction("Index", "Participant");
            }

            return View();
        }

        public async Task<IActionResult> Events()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Organiser")
            {
                return RedirectToAction("Index", "Participant");
            }

            var result = await _apiService.GetAsync("api/events");

            if (!result.Success || result.Data == null)
            {
                ViewBag.ErrorMessage = "Unable to load your events.";
                return View(new List<EventViewModel>());
            }

            var events = new List<EventViewModel>();

            if (result.Data.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in result.Data.Value.EnumerateArray())
                {
                    int organiserId = item.GetProperty("organiserID").GetInt32();

                    if (organiserId != userId.Value)
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
                        EventDate = item.GetProperty("eventDate").GetDateTime(),
                        StartTime = TimeSpan.Parse(
                            item.GetProperty("startTime").GetString() ?? "00:00:00"),
                        Location = item.GetProperty("location").GetString() ?? "",
                        DistanceKm = item.GetProperty("distanceKm").GetDecimal(),
                        EventType = item.GetProperty("eventType").GetString() ?? "",
                        Status = item.GetProperty("status").GetString() ?? ""
                    });
                }
            }

            return View(events.OrderBy(e => e.EventDate).ToList());
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Organiser")
            {
                return RedirectToAction("Index", "Participant");
            }

            return View(new CreateEventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Organiser")
            {
                return RedirectToAction("Index", "Participant");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!TimeSpan.TryParse(model.StartTime, out TimeSpan startTime))
            {
                ModelState.AddModelError("StartTime", "Please enter a valid start time.");
                return View(model);
            }

            if (model.EventDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("EventDate", "The event date cannot be in the past.");
                return View(model);
            }

            var eventData = new
            {
                EventName = model.EventName,
                Description = model.Description,
                EventDate = model.EventDate,
                StartTime = startTime,
                Location = model.Location,
                DistanceKm = model.DistanceKm,
                EventType = model.EventType
            };

            var result = await _apiService.PostAsync<object>("api/events", eventData);

            if (!result.Success)
            {
                ModelState.AddModelError("", string.IsNullOrWhiteSpace(result.Message)
                    ? "Unable to create the event. Please try again."
                    : result.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = "Your event was created successfully.";

            return RedirectToAction("Events");
        }

        // POST: Upload Event Banner
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Participant");

            // Check that the event exists and belongs to this organiser.
            var eventResponse = await _apiService.GetAsync($"api/events/{id}");

            if (!eventResponse.Success || eventResponse.Data == null)
            {
                TempData["ErrorMessage"] = "Unable to find the selected event.";
                return RedirectToAction("Events");
            }

            int organiserId = eventResponse.Data.Value
                .GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return Forbid();

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image.";
                return RedirectToAction("EditEvent", new { id });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "The image must not exceed 5 MB.";
                return RedirectToAction("EditEvent", new { id });
            }

            string contentType = file.ContentType.ToLowerInvariant();

            string[] allowedTypes =
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            if (!allowedTypes.Contains(contentType))
            {
                TempData["ErrorMessage"] =
                    "Only JPEG, PNG and WebP images are allowed.";

                return RedirectToAction("EditEvent", new { id });
            }

            using var stream = file.OpenReadStream();

            var result = await _apiService.UploadFileAsync(
                $"api/events/{id}/banner",
                stream,
                file.FileName,
                contentType);

            if (!result.Success)
            {
                TempData["ErrorMessage"] =
                    string.IsNullOrWhiteSpace(result.Message)
                        ? "Unable to upload the event banner."
                        : result.Message;

                return RedirectToAction("EditEvent", new { id });
            }

            TempData["SuccessMessage"] =
                "Event banner uploaded successfully.";

            return RedirectToAction("EditEvent", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Participant");

            var result = await _apiService.GetAsync($"api/events/{id}");

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = "Unable to find the selected event.";
                return RedirectToAction("Events");
            }

            var item = result.Data.Value;

            var model = new CreateEventViewModel
            {
                EventName = item.GetProperty("eventName").GetString() ?? "",
                Description = item.TryGetProperty("description", out var description)
                    ? description.GetString()
                    : null,
                EventDate = item.GetProperty("eventDate").GetDateTime(),
                StartTime = TimeSpan.Parse(
                    item.GetProperty("startTime").GetString() ?? "00:00:00"
                ).ToString(@"hh\:mm"),
                Location = item.GetProperty("location").GetString() ?? "",
                DistanceKm = item.GetProperty("distanceKm").GetDecimal(),
                EventType = item.GetProperty("eventType").GetString() ?? ""
            };

            ViewBag.EventID = id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, CreateEventViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Participant");

            ViewBag.EventID = id;

            if (!ModelState.IsValid)
                return View(model);

            if (!TimeSpan.TryParse(model.StartTime, out TimeSpan startTime))
            {
                ModelState.AddModelError("StartTime", "Please enter a valid start time.");
                return View(model);
            }

            if (model.EventDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("EventDate", "The event date cannot be in the past.");
                return View(model);
            }

            var eventData = new
            {
                EventName = model.EventName,
                Description = model.Description,
                EventDate = model.EventDate,
                StartTime = startTime,
                Location = model.Location,
                DistanceKm = model.DistanceKm,
                EventType = model.EventType
            };

            var result = await _apiService.PutAsync<object>(
                $"api/events/{id}",
                eventData
            );

            if (!result.Success)
            {
                ModelState.AddModelError(
                    "",
                    string.IsNullOrWhiteSpace(result.Message)
                        ? "Unable to update the event. Please try again."
                        : result.Message
                );

                return View(model);
            }

            TempData["SuccessMessage"] = "Your event was updated successfully.";

            return RedirectToAction("Events");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEvent(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Participant");

            var result = await _apiService.DeleteAsync($"api/events/{id}");

            if (!result.Success)
            {
                TempData["ErrorMessage"] =
                    string.IsNullOrWhiteSpace(result.Message)
                        ? "Unable to cancel the event. Please try again."
                        : result.Message;

                return RedirectToAction("Events");
            }

            TempData["SuccessMessage"] = "Event cancelled successfully.";

            return RedirectToAction("Events");
        }

        public async Task<IActionResult> Categories()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Participant");

            var result = await _apiService.GetAsync("api/events");

            if (!result.Success || result.Data == null)
            {
                ViewBag.ErrorMessage = "Unable to load your events.";
                return View(new List<EventViewModel>());
            }

            var events = new List<EventViewModel>();

            if (result.Data.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in result.Data.Value.EnumerateArray())
                {
                    int organiserId = item.GetProperty("organiserID").GetInt32();

                    if (organiserId != userId.Value)
                        continue;

                    events.Add(new EventViewModel
                    {
                        EventID = item.GetProperty("eventID").GetInt32(),
                        EventName = item.GetProperty("eventName").GetString() ?? "",
                        Description = item.TryGetProperty("description", out var description)
                            ? description.GetString()
                            : null,
                        EventDate = item.GetProperty("eventDate").GetDateTime(),
                        StartTime = TimeSpan.Parse(
                            item.GetProperty("startTime").GetString() ?? "00:00:00"),
                        Location = item.GetProperty("location").GetString() ?? "",
                        DistanceKm = item.GetProperty("distanceKm").GetDecimal(),
                        EventType = item.GetProperty("eventType").GetString() ?? "",
                        Status = item.GetProperty("status").GetString() ?? ""
                    });
                }
            }

            return View(events.OrderBy(e => e.EventDate).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategories(int eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            string? role = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            if (role != "Organiser")
                return RedirectToAction("Index", "Participant");

            // Load the selected event.
            var eventResult = await _apiService.GetAsync($"api/events/{eventId}");

            if (!eventResult.Success || eventResult.Data == null)
            {
                TempData["ErrorMessage"] = "Unable to find the selected event.";
                return RedirectToAction("Categories");
            }

            var eventData = eventResult.Data.Value;

            int organiserId = eventData.GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return RedirectToAction("Categories");

            ViewBag.EventID = eventId;
            ViewBag.EventName = eventData.GetProperty("eventName").GetString();

            // Load the categories belonging to this event.
            var result = await _apiService.GetAsync(
                $"api/events/{eventId}/categories");

            if (!result.Success || result.Data == null)
            {
                ViewBag.ErrorMessage = "Unable to load the race categories.";
                return View(new List<EventCategoryViewModel>());
            }

            var categories = new List<EventCategoryViewModel>();

            if (result.Data.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in result.Data.Value.EnumerateArray())
                {
                    var category = item.GetProperty("category");

                    categories.Add(new EventCategoryViewModel
                    {
                        EventCategoryID = item.GetProperty("eventCategoryID").GetInt32(),
                        EventID = item.GetProperty("eventID").GetInt32(),
                        CategoryID = item.GetProperty("categoryID").GetInt32(),
                        CategoryName = category.GetProperty("categoryName").GetString() ?? "",
                        DistanceKm = category.GetProperty("distanceKm").GetDecimal(),
                        Description = category.TryGetProperty("description", out var description)
                            ? description.GetString()
                            : null,
                        EntryFee = item.GetProperty("entryFee").GetDecimal(),
                        MaxParticipants = item.GetProperty("maxParticipants").GetInt32(),
                        AvailableSlots = item.GetProperty("availableSlots").GetInt32()
                    });
                }
            }

            return View(categories);
        }

        // GET: Add Category
        [HttpGet]
        public async Task<IActionResult> AddCategory(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            var eventResponse = await _apiService.GetAsync($"api/events/{eventId}");

            if (!eventResponse.Success || eventResponse.Data == null)
                return NotFound();

            var organiserId = eventResponse.Data.Value
                .GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return Forbid();

            var model = new AddCategoryViewModel
            {
                EventID = eventId
            };

            return View(model);
        }


        // POST: Add Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var request = new
            {
                model.CategoryName,
                model.DistanceKm,
                model.Description,
                model.EntryFee,
                model.MaxParticipants
            };

            var response = await _apiService.PostAsync(
                $"api/events/{model.EventID}/categories",
                request);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Race category added successfully.";

                return RedirectToAction("ManageCategories",
                    new { eventId = model.EventID });
            }

            ModelState.AddModelError(
                string.Empty,
                response.Message ?? "Unable to add the category. Please try again.");

            return View(model);
        }

        // GET: Edit Category
        [HttpGet]
        public async Task<IActionResult> EditCategory(int eventId, int categoryId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            // Check that the event belongs to this organiser.
            var eventResponse = await _apiService.GetAsync($"api/events/{eventId}");

            if (!eventResponse.Success || eventResponse.Data == null)
                return NotFound();

            var organiserId = eventResponse.Data.Value
                .GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return Forbid();

            // Retrieve the categories for this event.
            var response = await _apiService.GetAsync(
                $"api/events/{eventId}/categories");

            if (!response.Success || response.Data == null)
                return NotFound();

            var category = response.Data.Value
                .EnumerateArray()
                .FirstOrDefault(item =>
                    item.GetProperty("eventCategoryID").GetInt32() == categoryId);

            if (category.ValueKind == System.Text.Json.JsonValueKind.Undefined)
                return NotFound();

            var nestedCategory = category.GetProperty("category");

            var model = new EventCategoryViewModel
            {
                EventCategoryID = category.GetProperty("eventCategoryID").GetInt32(),
                EventID = eventId,
                CategoryID = category.GetProperty("categoryID").GetInt32(),
                CategoryName = nestedCategory.GetProperty("categoryName").GetString() ?? "",
                DistanceKm = nestedCategory.GetProperty("distanceKm").GetDecimal(),
                Description = nestedCategory.TryGetProperty("description", out var description)
                    ? description.GetString()
                    : null,
                EntryFee = category.GetProperty("entryFee").GetDecimal(),
                MaxParticipants = category.GetProperty("maxParticipants").GetInt32(),
                AvailableSlots = category.GetProperty("availableSlots").GetInt32()
            };

            return View(model);
        }


        // POST: Edit Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(EventCategoryViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var request = new
            {
                model.CategoryName,
                model.DistanceKm,
                model.Description,
                model.EntryFee,
                model.MaxParticipants
            };

            var response = await _apiService.PutAsync(
                $"api/events/{model.EventID}/categories/{model.EventCategoryID}",
                request);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Race category updated successfully.";

                return RedirectToAction("ManageCategories",
                    new { eventId = model.EventID });
            }

            ModelState.AddModelError(
                string.Empty,
                response.Message ?? "Unable to update the category. Please try again.");

            return View(model);
        }

        // POST: Delete Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            // Confirm that the event belongs to this organiser.
            var eventResponse = await _apiService.GetAsync($"api/events/{eventId}");

            if (!eventResponse.Success || eventResponse.Data == null)
                return NotFound();

            var organiserId = eventResponse.Data.Value
                .GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return Forbid();

            var response = await _apiService.DeleteAsync(
                $"api/events/{eventId}/categories/{categoryId}");

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Race category deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    response.Message ?? "Unable to delete the race category.";
            }

            return RedirectToAction("ManageCategories",
                new { eventId });
        }

        // GET: View Event Enrolments
        [HttpGet]
        public async Task<IActionResult> Enrolments(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            // Get the event details.
            var eventResponse = await _apiService.GetAsync(
                $"api/events/{eventId}");

            if (!eventResponse.Success || eventResponse.Data == null)
                return NotFound();

            var organiserId = eventResponse.Data.Value
                .GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return Forbid();

            // Retrieve enrolments from the API.
            var response = await _apiService.GetAsync(
                $"api/events/{eventId}/enrolments");

            if (!response.Success || response.Data == null)
            {
                ViewBag.ErrorMessage = response.Message;
                return View(new List<EnrolmentViewModel>());
            }

            var enrolments = new List<EnrolmentViewModel>();

            foreach (var item in response.Data.Value.EnumerateArray())
            {
                var participant = item.GetProperty("participant");
                var eventCategory = item.GetProperty("eventCategory");
                var category = eventCategory.GetProperty("category");

                var firstName = participant.GetProperty("firstName").GetString() ?? "";
                var lastName = participant.GetProperty("lastName").GetString() ?? "";

                enrolments.Add(new EnrolmentViewModel
                {
                    EnrolmentID = item.GetProperty("enrolmentID").GetInt32(),
                    ParticipantID = item.GetProperty("participantID").GetInt32(),
                    ParticipantName = $"{firstName} {lastName}".Trim(),
                    ParticipantEmail = participant.GetProperty("email").GetString() ?? "",
                    ParticipantPhone = participant.TryGetProperty("phone", out var phone)
                        ? phone.GetString() ?? ""
                        : "",
                    CategoryName = category.GetProperty("categoryName").GetString() ?? "",
                    DistanceKm = category.GetProperty("distanceKm").GetDecimal(),
                    RaceNumber = item.GetProperty("raceNumber").GetInt32(),
                    EnrolmentStatus = item.GetProperty("enrolmentStatus").GetString() ?? ""
                });
            }

            ViewBag.EventID = eventId;
            ViewBag.EventName = eventResponse.Data.Value
                .GetProperty("eventName").GetString();

            return View(enrolments);
        }

        // GET: Record Participant Result
        [HttpGet]
        public async Task<IActionResult> RecordResult(int eventId, int enrolmentId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            // Verify event ownership.
            var eventResponse = await _apiService.GetAsync(
                $"api/events/{eventId}");

            if (!eventResponse.Success || eventResponse.Data == null)
                return NotFound();

            var organiserId = eventResponse.Data.Value
                .GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return Forbid();

            // Retrieve the event enrolments.
            var response = await _apiService.GetAsync(
                $"api/events/{eventId}/enrolments");

            if (!response.Success || response.Data == null)
                return NotFound();

            var enrolment = response.Data.Value
                .EnumerateArray()
                .FirstOrDefault(item =>
                    item.GetProperty("enrolmentID").GetInt32() == enrolmentId);

            if (enrolment.ValueKind == System.Text.Json.JsonValueKind.Undefined)
                return NotFound();

            var participant = enrolment.GetProperty("participant");

            var firstName = participant.GetProperty("firstName").GetString() ?? "";
            var lastName = participant.GetProperty("lastName").GetString() ?? "";

            var model = new ResultViewModel
            {
                EnrolmentID = enrolmentId,
                ParticipantID = enrolment.GetProperty("participantID").GetInt32(),
                ParticipantName = $"{firstName} {lastName}".Trim(),
                RaceNumber = enrolment.GetProperty("raceNumber").GetInt32(),
                ResultStatus = "Finished"
            };

            ViewBag.EventID = eventId;
            ViewBag.EventName = eventResponse.Data.Value
                .GetProperty("eventName").GetString();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(
            int eventId,
            ResultViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Account");

            var eventResponse = await _apiService.GetAsync(
                $"api/events/{eventId}");

            if (!eventResponse.Success || eventResponse.Data == null)
                return NotFound("Event not found.");

            var eventData = eventResponse.Data.Value;

            ViewBag.EventID = eventId;

            if (eventData.TryGetProperty("eventName", out var eventName))
                ViewBag.EventName = eventName.GetString();

            TimeSpan? finishTime = null;
            int? position = null;

            if (model.ResultStatus == "Finished")
            {
                if (string.IsNullOrWhiteSpace(model.FinishTime) ||
                    !TimeSpan.TryParse(model.FinishTime, out var parsedTime))
                {
                    ModelState.AddModelError(
                        nameof(model.FinishTime),
                        "Enter a valid finish time.");
                }
                else
                {
                    finishTime = parsedTime;
                }

                if (!model.Position.HasValue || model.Position.Value < 1)
                {
                    ModelState.AddModelError(
                        nameof(model.Position),
                        "Enter a valid finishing position.");
                }
                else
                {
                    position = model.Position;
                }
            }
            else if (model.ResultStatus != "DNS" &&
                     model.ResultStatus != "DNF")
            {
                ModelState.AddModelError(
                    nameof(model.ResultStatus),
                    "Select a valid result status.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var request = new
            {
                FinishTime = finishTime,
                Position = position,
                ResultStatus = model.ResultStatus
            };

            var response = await _apiService.PostAsync(
                $"api/enrolments/{model.EnrolmentID}/result",
                request);

            if (!response.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    response.Message);

                return View(model);
            }

            TempData["SuccessMessage"] = "Race result saved successfully.";

            return RedirectToAction(
                "Enrolments",
                new { eventId = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int eventId,
            int enrolmentId,
            string enrolmentStatus)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Account");

            var request = new
            {
                EnrolmentStatus = enrolmentStatus
            };

            var response = await _apiService.PutAsync(
                $"api/enrolments/{enrolmentId}/status",
                request);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
            }
            else
            {
                TempData["SuccessMessage"] =
                    $"Enrolment successfully {enrolmentStatus.ToLower()}.";
            }

            return RedirectToAction(
                "Enrolments",
                new { eventId = eventId });
        }

        // GET: View Event Results
        [HttpGet]
        public async Task<IActionResult> EventResults(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Organiser")
                return RedirectToAction("Login", "Account");

            // Verify that the event belongs to this organiser.
            var eventResponse = await _apiService.GetAsync(
                $"api/events/{eventId}");

            if (!eventResponse.Success || eventResponse.Data == null)
                return NotFound();

            var eventData = eventResponse.Data.Value;

            var organiserId = eventData
                .GetProperty("organiserID").GetInt32();

            if (organiserId != userId.Value)
                return Forbid();

            ViewBag.EventID = eventId;
            ViewBag.EventName = eventData
                .GetProperty("eventName").GetString();

            // Retrieve recorded results from the API.
            var response = await _apiService.GetAsync(
                $"api/events/{eventId}/results");

            if (!response.Success || response.Data == null)
            {
                ViewBag.ErrorMessage = "Unable to load event results.";

                return View(new List<ResultViewModel>());
            }

            var results = new List<ResultViewModel>();

            foreach (var item in response.Data.Value.EnumerateArray())
            {
                var enrolment = item.GetProperty("enrolment");
                var participant = enrolment.GetProperty("participant");

                var firstName = participant.GetProperty("firstName").GetString() ?? "";
                var lastName = participant.GetProperty("lastName").GetString() ?? "";

                var model = new ResultViewModel
                {
                    EnrolmentID = enrolment.GetProperty("enrolmentID").GetInt32(),
                    ParticipantID = enrolment.GetProperty("participantID").GetInt32(),
                    ParticipantName = $"{firstName} {lastName}".Trim(),
                    RaceNumber = enrolment.GetProperty("raceNumber").GetInt32(),
                    ResultStatus = item.GetProperty("resultStatus").GetString() ?? ""
                };

                if (item.TryGetProperty("finishTime", out var finishTime) &&
                    finishTime.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    model.FinishTime = finishTime.GetString() ?? "";
                }

                if (item.TryGetProperty("position", out var position) &&
                    position.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    model.Position = position.GetInt32();
                }

                results.Add(model);
            }

            return View(results.OrderBy(r => r.Position ?? int.MaxValue).ToList());
        }
    }
}