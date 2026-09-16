namespace RaceDay.Api.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }
        public EventEnrolment? Enrolment { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        // "Finished", "DNF", or "DNS"
        public string Status { get; set; } = "Finished";
    }
}