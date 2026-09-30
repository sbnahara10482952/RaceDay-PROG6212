using Microsoft.AspNetCore.Mvc;

namespace RaceDay.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected int? CurrentUserId =>
            HttpContext.Session.GetInt32("UserID");

        protected string? CurrentRole =>
            HttpContext.Session.GetString("Role");

        protected bool IsLoggedIn()
        {
            return CurrentUserId.HasValue;
        }

        protected bool IsRole(string role)
        {
            return CurrentRole == role;
        }
    }
}