using RaceDay.Api.DTOs;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthControllerTests : IClassFixture<RaceDayApiFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(RaceDayApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            var dto = new RegisterDto
            {
                Name = "Thabo Nkosi",
                Email = "thabo.test@raceday.co.za",
                Password = "SecurePass123",
                Role = "Organiser",
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var dto = new RegisterDto
            {
                Name = "Lerato Dube",
                Email = "duplicate@raceday.co.za",
                Password = "SecurePass123",
                Role = "Participant",
            };

            await _client.PostAsJsonAsync("/api/auth/register", dto);
            var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidRole_ReturnsBadRequest()
        {
            var dto = new RegisterDto
            {
                Name = "Invalid Role User",
                Email = "invalidrole@raceday.co.za",
                Password = "SecurePass123",
                Role = "Administrator",
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var register = new RegisterDto
            {
                Name = "Sipho Mahlangu",
                Email = "sipho.login@raceday.co.za",
                Password = "CorrectPass123",
                Role = "Participant",
            };
            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginDto { Email = register.Email, Password = register.Password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var register = new RegisterDto
            {
                Name = "Anika van Wyk",
                Email = "anika.login@raceday.co.za",
                Password = "CorrectPass123",
                Role = "Participant",
            };
            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginDto { Email = register.Email, Password = "WrongPassword" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithNonexistentEmail_ReturnsUnauthorized()
        {
            var login = new LoginDto { Email = "doesnotexist@raceday.co.za", Password = "Whatever123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}