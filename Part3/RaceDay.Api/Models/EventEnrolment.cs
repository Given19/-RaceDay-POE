namespace RaceDay.Api.Models
{
    public class EventEnrolment
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }
        public User? Participant { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        // "Confirmed" or "Cancelled"
        public string Status { get; set; } = "Confirmed";

        public Result? Result { get; set; }
    }
}