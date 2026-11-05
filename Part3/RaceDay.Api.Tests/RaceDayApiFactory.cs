using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using System.Linq;

namespace RaceDay.Api.Tests
{
    public class RaceDayApiFactory : WebApplicationFactory<Program>
    {
        public string DbName { get; } = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RaceDayContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<RaceDayContext>(options =>
                    options.UseInMemoryDatabase(DbName));

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, options => { });
            });
        }
    }
}