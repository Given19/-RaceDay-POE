using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class EnrolmentResponseDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public ResultResponseDto? Result { get; set; }
    }

    public class ResultCreateDto
    {
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }

        // "Finished", "DNF", or "DNS"
        [Required]
        public string Status { get; set; } = "Finished";
    }

    public class ResultResponseDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}