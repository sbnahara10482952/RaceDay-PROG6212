using System.Text.Json.Serialization;

namespace RaceDay.API.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? ProfileImageUrl { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}