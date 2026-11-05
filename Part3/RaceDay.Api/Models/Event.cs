using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public int OrganiserId { get; set; }
        public User? Organiser { get; set; }

        [Required, MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Added in Part 2 per the brief's functional requirements
        // (Part 1 only captured distance at Category level; the Part 2 brief
        // explicitly asks for a top-level event distance and event type).
        [Required]
        public decimal Distance { get; set; }

        // "Run", "Walk", or "Cycle"
        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}