using RaceDay.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EnrolmentsControllerTests : IClassFixture<RaceDayApiFactory>
    {
        private readonly HttpClient _client;

        public EnrolmentsControllerTests(RaceDayApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<int> CreateEventAndCategoryAsOrganiser(int organiserId)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", organiserId.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Organiser");

            var eventDto = new EventCreateDto
            {
                EventName = "Soweto Fun Run",
                EventDate = DateTime.UtcNow.AddMonths(1),
                Location = "Soweto, Gauteng",
                Description = "Community fun run.",
                Distance = 10m,
                EventType = "Run",
            };
            var eventResponse = await _client.PostAsJsonAsync("/api/events", eventDto);
            var ev = await eventResponse.Content.ReadFromJsonAsync<EventResponseDto>();

            var categoryDto = new CategoryCreateDto { CategoryName = "10km Run", Distance = 10m, EntryFee = 150m };
            var categoryResponse = await _client.PostAsJsonAsync($"/api/events/{ev!.EventId}/categories", categoryDto);
            var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();

            return category!.CategoryId;
        }

        [Fact]
        public async Task Enrol_AsParticipant_ReturnsCreatedAndIsRecorded()
        {
            int categoryId = await CreateEventAndCategoryAsOrganiser(organiserId: 20);

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "30");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Participant");

            var enrolResponse = await _client.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            Assert.Equal(HttpStatusCode.Created, enrolResponse.StatusCode);

            var enrolment = await enrolResponse.Content.ReadFromJsonAsync<EnrolmentResponseDto>();
            Assert.Equal(30, enrolment!.ParticipantId);
            Assert.Equal(categoryId, enrolment.CategoryId);

            var historyResponse = await _client.GetAsync("/api/users/30/enrolments");
            Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
            var history = await historyResponse.Content.ReadFromJsonAsync<List<EnrolmentResponseDto>>();
            Assert.Contains(history!, e => e.EnrolmentId == enrolment.EnrolmentId);
        }

        [Fact]
        public async Task Enrol_AsOrganiser_ReturnsForbidden()
        {
            int categoryId = await CreateEventAndCategoryAsOrganiser(organiserId: 21);

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "21");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Organiser");

            var response = await _client.PostAsync($"/api/categories/{categoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_Twice_ReturnsConflict()
        {
            int categoryId = await CreateEventAndCategoryAsOrganiser(organiserId: 22);

            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "31");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Participant");

            var first = await _client.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            var second = await _client.PostAsync($"/api/categories/{categoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Created, first.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }

        [Fact]
        public async Task GetEnrolmentHistory_ForAnotherUser_ReturnsForbidden()
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", "40");
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Participant");

            var response = await _client.GetAsync("/api/users/999/enrolments");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}