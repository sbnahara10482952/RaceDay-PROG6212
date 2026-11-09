using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your first name.")]
        public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your last name.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a password.")]
        [MinLength(8, ErrorMessage = "Your password must have at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
    }

}
