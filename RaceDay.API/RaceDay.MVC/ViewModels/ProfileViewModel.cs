using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class ProfileViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        [Display(Name = "Date Registered")]
        public DateTime DateCreated { get; set; }
    }
}