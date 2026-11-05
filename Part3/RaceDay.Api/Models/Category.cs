using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        // e.g. "Under 20", "Senior", "10km", "21km"
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public decimal Distance { get; set; }

        public decimal EntryFee { get; set; }

        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}