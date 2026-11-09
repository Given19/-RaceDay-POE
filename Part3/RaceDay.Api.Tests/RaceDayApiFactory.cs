using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

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

                // Tests never talk to Azure.
                services.RemoveAll<IBlobStorageService>();
                services.AddSingleton<IBlobStorageService, FakeBlobStorageService>();

                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, options => { });
            });
        }
    }
}