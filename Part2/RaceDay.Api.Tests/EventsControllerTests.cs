using RaceDay.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventsControllerTests : IClassFixture<RaceDayApiFactory>
    {
        private readonly HttpClient _client;

        public EventsControllerTests(RaceDayApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static EventCreateDto SampleEvent() => new()
        {
            EventName = "Johannesburg City Marathon",
            EventDate = DateTime.UtcNow.AddMonths(2),
            Location = "Johannesburg, Gauteng",
            Description = "Annual road marathon.",
            Distance = 42.2m,
            EventType = "Run",
        };

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "1");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Organiser");

            var response = await _client.PostAsJsonAsync("/api/events", SampleEvent());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "2");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Participant");

            var response = await _client.PostAsJsonAsync("/api/events", SampleEvent());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_Unauthenticated_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");

            var response = await _client.PostAsJsonAsync("/api/events", SampleEvent());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllEvents_Unauthenticated_ReturnsOk()
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");

            var response = await _client.GetAsync("/api/events");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_AsNonOwnerOrganiser_ReturnsForbidden()
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "10");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Organiser");
            var createResponse = await _client.PostAsJsonAsync("/api/events", SampleEvent());
            var created = await createResponse.Content.ReadFromJsonAsync<EventResponseDto>();

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "11");

            var updateResponse = await _client.PutAsJsonAsync($"/api/events/{created!.EventId}", SampleEvent());

            Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        }
    }
}