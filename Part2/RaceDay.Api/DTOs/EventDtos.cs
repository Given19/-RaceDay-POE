using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class EventCreateDto
    {
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
        [Required]
        public string EventType { get; set; } = string.Empty;
    }

    public class EventResponseDto
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    public class CategoryCreateDto
    {
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        public decimal EntryFee { get; set; } = 0;
    }

    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public decimal EntryFee { get; set; }
    }
}