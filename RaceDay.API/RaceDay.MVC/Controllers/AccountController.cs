using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public AccountController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserID").HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _apiService.LoginAsync(
                model.Email,
                model.Password);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            string? role = HttpContext.Session.GetString("Role");

            if (role == "Organiser")
            {
                return RedirectToAction("Index", "Organiser");
            }

            if (role == "Participant")
            {
                return RedirectToAction("Index", "Participant");
            }

            HttpContext.Session.Clear();

            ModelState.AddModelError(
                string.Empty,
                "Your account has an unrecognised role.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _apiService.LogoutAsync();

            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("UserID").HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _apiService.RegisterAsync(
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.PhoneNumber);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Registration successful. You can now log in.";

            return RedirectToAction("Login");
        }

    }
}
