using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class EventCategoryViewModel
    {
        public int EventCategoryID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the race distance.")]
        [Range(0.1, 100, ErrorMessage = "Distance must be between 0.1 and 100 km.")]
        public decimal DistanceKm { get; set; }

        public string? Description { get; set; }

        [Range(0, 100000, ErrorMessage = "Entry fee cannot be negative.")]
        public decimal EntryFee { get; set; }

        [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
        public int MaxParticipants { get; set; }

        public int AvailableSlots { get; set; }
    }
}