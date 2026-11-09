namespace RaceDay.MVC.ViewModels
{
    public class EnrolmentViewModel
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public string ParticipantEmail { get; set; } = string.Empty;

        public string ParticipantPhone { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public int RaceNumber { get; set; }

        public string EnrolmentStatus { get; set; } = string.Empty;

        public DateTime EnrolmentDate { get; set; }
    }
}