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

        [Required]
        public decimal Distance { get; set; }

        // "Run", "Walk", or "Cycle"
        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        // URL of the event banner stored in Azure Blob Storage
        [MaxLength(500)]
        public string? BannerUrl { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}