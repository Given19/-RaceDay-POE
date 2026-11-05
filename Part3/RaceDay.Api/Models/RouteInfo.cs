namespace RaceDay.Api.Models
{
    public class RouteInfo
    {
        public int RouteId { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? RouteDescription { get; set; }

        public string? MapUrl { get; set; }
    }
}