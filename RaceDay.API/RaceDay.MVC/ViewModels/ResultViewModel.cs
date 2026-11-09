using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class ResultViewModel
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public int RaceNumber { get; set; }

        [DataType(DataType.Time)]
        public string FinishTime { get; set; } = string.Empty;

        [Range(1, 100000, ErrorMessage = "Position must be at least 1.")]
        public int? Position { get; set; }

        [Required(ErrorMessage = "Please select a result status.")]
        public string ResultStatus { get; set; } = "Finished";

        public string CategoryName { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;
    }
}