using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class AddCategoryViewModel
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the race distance.")]
        [Range(0.1, 100, ErrorMessage = "Distance must be between 0.1 and 100 km.")]
        public decimal DistanceKm { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 100000, ErrorMessage = "Entry fee cannot be negative.")]
        public decimal EntryFee { get; set; }

        [Required(ErrorMessage = "Please enter the maximum number of participants.")]
        [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
        public int MaxParticipants { get; set; }
    }
}