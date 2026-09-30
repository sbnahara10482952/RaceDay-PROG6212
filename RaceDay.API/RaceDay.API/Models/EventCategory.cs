namespace RaceDay.API.Models
{
    public class EventCategory
    {
        public int EventCategoryID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public decimal EntryFee { get; set; }

        public int MaxParticipants { get; set; }

        public int AvailableSlots { get; set; }

        // Relationships
        public Event? Event { get; set; }

        public Category? Category { get; set; }
    }
}