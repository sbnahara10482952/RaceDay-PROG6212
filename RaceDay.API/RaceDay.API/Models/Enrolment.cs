namespace RaceDay.API.Models
{
    public class Enrolment
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventCategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        public string EnrolmentStatus { get; set; } = string.Empty;

        public int RaceNumber { get; set; }

        // Relationships
        public User? Participant { get; set; }

        public EventCategory? EventCategory { get; set; }

        public Result? Result { get; set; }
    }
}