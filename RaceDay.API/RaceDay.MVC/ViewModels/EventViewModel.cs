namespace RaceDay.MVC.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }

    public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Location { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }

        public List<EventCategoryViewModel> Categories { get; set; }
        = new List<EventCategoryViewModel>();
    }

}
