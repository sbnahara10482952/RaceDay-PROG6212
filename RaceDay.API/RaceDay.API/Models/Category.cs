namespace RaceDay.API.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public string? Description { get; set; }

        // Relationship with event categories
        public ICollection<EventCategory> EventCategories { get; set; }
            = new List<EventCategory>();
    }
}