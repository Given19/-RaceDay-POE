using RaceDay.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class Part3FeatureTests : IClassFixture<RaceDayApiFactory>
    {
        private readonly HttpClient _client;

        public Part3FeatureTests(RaceDayApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        // ---- helpers ----
        private void LoginAs(int userId, string role)
        {
            _client.DefaultRequestHeaders.Remove("X-Test-UserId");
            _client.DefaultRequestHeaders.Remove("X-Test-Role");
            _client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
            _client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        private async Task<EventResponseDto> CreateEvent(int organiserId, string type = "Run", string name = "Test Event")
        {
            LoginAs(organiserId, "Organiser");
            var dto = new EventCreateDto
            {
                EventName = name,
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Pretoria",
                Description = "Test event",
                Distance = 10m,
                EventType = type,
            };
            var response = await _client.PostAsJsonAsync("/api/events", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<EventResponseDto>())!;
        }

        private async Task<CategoryResponseDto> CreateCategory(int organiserId, int eventId)
        {
            LoginAs(organiserId, "Organiser");
            var response = await _client.PostAsJsonAsync($"/api/events/{eventId}/categories",
                new CategoryCreateDto { CategoryName = "10km", Distance = 10m, EntryFee = 100m });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<CategoryResponseDto>())!;
        }

        private async Task<EnrolmentResponseDto> Enrol(int participantId, int categoryId)
        {
            LoginAs(participantId, "Participant");
            var response = await _client.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<EnrolmentResponseDto>())!;
        }

        private static MultipartFormDataContent ImageUpload(string fileName, string contentType)
        {
            var form = new MultipartFormDataContent();
            var file = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            form.Add(file, "file", fileName);
            return form;
        }

        // ---- enrolment status ----
        [Fact]
        public async Task NewEnrolment_StartsAsPending()
        {
            var ev = await CreateEvent(200);
            var category = await CreateCategory(200, ev.EventId);

            var enrolment = await Enrol(300, category.CategoryId);

            Assert.Equal("Pending", enrolment.Status);
        }

        [Fact]
        public async Task Organiser_CanConfirmEnrolment()
        {
            var ev = await CreateEvent(201);
            var category = await CreateCategory(201, ev.EventId);
            var enrolment = await Enrol(301, category.CategoryId);

            LoginAs(201, "Organiser");
            var confirm = await _client.PutAsync($"/api/enrolments/{enrolment.EnrolmentId}/confirm", null);
            Assert.Equal(HttpStatusCode.OK, confirm.StatusCode);

            var list = await _client.GetFromJsonAsync<List<EnrolmentResponseDto>>($"/api/events/{ev.EventId}/enrolments");
            Assert.Equal("Confirmed", list!.Single(e => e.EnrolmentId == enrolment.EnrolmentId).Status);
        }

        [Fact]
        public async Task Participant_CannotConfirmEnrolment()
        {
            var ev = await CreateEvent(202);
            var category = await CreateCategory(202, ev.EventId);
            var enrolment = await Enrol(302, category.CategoryId);

            LoginAs(302, "Participant");
            var response = await _client.PutAsync($"/api/enrolments/{enrolment.EnrolmentId}/confirm", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task OtherOrganiser_CannotConfirmEnrolment()
        {
            var ev = await CreateEvent(203);
            var category = await CreateCategory(203, ev.EventId);
            var enrolment = await Enrol(303, category.CategoryId);

            LoginAs(999, "Organiser");
            var response = await _client.PutAsync($"/api/enrolments/{enrolment.EnrolmentId}/confirm", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CanReEnrolAfterCancelling()
        {
            var ev = await CreateEvent(204);
            var category = await CreateCategory(204, ev.EventId);
            var enrolment = await Enrol(304, category.CategoryId);

            var cancel = await _client.DeleteAsync($"/api/enrolments/{enrolment.EnrolmentId}");
            Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);

            var again = await Enrol(304, category.CategoryId);

            Assert.Equal("Pending", again.Status);
            Assert.Equal(enrolment.EnrolmentId, again.EnrolmentId);
        }

        // ---- images (Azure Blob Storage, faked in tests) ----
        [Fact]
        public async Task Organiser_CanUploadBanner()
        {
            var ev = await CreateEvent(210);
            LoginAs(210, "Organiser");

            var response = await _client.PostAsync($"/api/events/{ev.EventId}/banner", ImageUpload("banner.png", "image/png"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<EventResponseDto>();
            Assert.StartsWith("https://fake-storage.test/banners/", updated!.BannerUrl);
        }

        [Fact]
        public async Task Participant_CannotUploadBanner()
        {
            var ev = await CreateEvent(211);
            LoginAs(311, "Participant");

            var response = await _client.PostAsync($"/api/events/{ev.EventId}/banner", ImageUpload("banner.png", "image/png"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UploadBanner_WithNonImage_ReturnsBadRequest()
        {
            var ev = await CreateEvent(212);
            LoginAs(212, "Organiser");

            var response = await _client.PostAsync($"/api/events/{ev.EventId}/banner", ImageUpload("notes.txt", "text/plain"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CanUploadProfilePicture()
        {
            var register = await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto
            {
                Name = "Pic User",
                Email = "pic.user@raceday.test",
                Password = "Secret123",
                Role = "Participant",
            });
            var user = await register.Content.ReadFromJsonAsync<UserResponseDto>();
            LoginAs(user!.UserId, "Participant");

            var response = await _client.PostAsync($"/api/users/{user.UserId}/profile-picture", ImageUpload("me.jpg", "image/jpeg"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<UserResponseDto>();
            Assert.StartsWith("https://fake-storage.test/profiles/", updated!.ProfilePictureUrl);
        }

        [Fact]
        public async Task Participant_CannotUploadPictureForAnotherUser()
        {
            LoginAs(400, "Participant");

            var response = await _client.PostAsync("/api/users/401/profile-picture", ImageUpload("me.jpg", "image/jpeg"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ---- organiser dashboard and event filtering ----
        [Fact]
        public async Task Organiser_SeesOnlyOwnEvents()
        {
            await CreateEvent(240, name: "Mine A");
            await CreateEvent(240, name: "Mine B");
            await CreateEvent(241, name: "Someone else");

            LoginAs(240, "Organiser");
            var list = await _client.GetFromJsonAsync<List<EventResponseDto>>("/api/events/mine");

            Assert.Equal(2, list!.Count);
            Assert.All(list, e => Assert.Equal(240, e.OrganiserId));
        }

        [Fact]
        public async Task Mine_IncludesEnrolmentCount()
        {
            var ev = await CreateEvent(245);
            var category = await CreateCategory(245, ev.EventId);
            await Enrol(345, category.CategoryId);
            await Enrol(346, category.CategoryId);

            LoginAs(245, "Organiser");
            var list = await _client.GetFromJsonAsync<List<EventResponseDto>>("/api/events/mine");

            Assert.Equal(2, Assert.Single(list!).EnrolmentCount);
        }

        [Fact]
        public async Task Events_CanBeFilteredByType()
        {
            var run = await CreateEvent(250, "Run", "Filter Run");
            var cycle = await CreateEvent(250, "Cycle", "Filter Cycle");

            var list = await _client.GetFromJsonAsync<List<EventResponseDto>>("/api/events?type=Cycle");

            Assert.All(list!, e => Assert.Equal("Cycle", e.EventType));
            Assert.Contains(list!, e => e.EventId == cycle.EventId);
            Assert.DoesNotContain(list!, e => e.EventId == run.EventId);
        }

        // ---- participant results ----
        [Fact]
        public async Task MyEnrolments_ShowResultAndFinisherCount()
        {
            var ev = await CreateEvent(260);
            var category = await CreateCategory(260, ev.EventId);
            var first = await Enrol(361, category.CategoryId);
            var second = await Enrol(362, category.CategoryId);

            LoginAs(260, "Organiser");
            var r1 = await _client.PostAsJsonAsync($"/api/enrolments/{first.EnrolmentId}/result",
                new ResultCreateDto { FinishTime = TimeSpan.FromMinutes(50), Position = 1, Status = "Finished" });
            var r2 = await _client.PostAsJsonAsync($"/api/enrolments/{second.EnrolmentId}/result",
                new ResultCreateDto { FinishTime = TimeSpan.FromMinutes(55), Position = 2, Status = "Finished" });
            Assert.Equal(HttpStatusCode.Created, r1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, r2.StatusCode);

            LoginAs(361, "Participant");
            var mine = await _client.GetFromJsonAsync<List<EnrolmentResponseDto>>("/api/users/361/enrolments");

            var enrolment = Assert.Single(mine!);
            Assert.Equal(1, enrolment.Result!.Position);
            Assert.Equal(2, enrolment.TotalFinishers);
        }
    }
}