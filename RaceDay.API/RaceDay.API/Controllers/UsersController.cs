using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseController
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            if (!IsLoggedIn())
                return Unauthorized();

            var user = await _context.Users.FindAsync(CurrentUserId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.UserID,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.DateCreated
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            if (!IsLoggedIn())
                return Unauthorized();

            var user = await _context.Users.FindAsync(CurrentUserId);

            if (user == null)
                return NotFound();

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}