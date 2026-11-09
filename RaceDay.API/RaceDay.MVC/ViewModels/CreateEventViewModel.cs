using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Please enter an event name.")]
        [StringLength(100, ErrorMessage = "The event name cannot exceed 100 characters.")]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "The description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select an event date.")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please select a start time.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the event location.")]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the race distance.")]
        [Range(0.1, 100, ErrorMessage = "Distance must be between 0.1 and 100 km.")]
        public decimal DistanceKm { get; set; }

        [Required(ErrorMessage = "Please select an event type.")]
        [RegularExpression("^(Run|Walk|Cycle)$",
            ErrorMessage = "Please select Run, Walk or Cycle.")]
        public string EventType { get; set; } = string.Empty;
    }
}