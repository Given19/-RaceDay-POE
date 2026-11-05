using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RaceDay.Api.Auth
{
    /// <summary>
    /// Authenticates each request from server-side session state (not from
    /// claims baked into the cookie itself). The cookie only carries a
    /// session ID; UserId and Role are looked up server-side on every request.
    /// </summary>
    public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "SessionAuth";

        public SessionAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userId = Context.Session.GetInt32("UserId");
            var role = Context.Session.GetString("Role");

            if (userId == null || string.IsNullOrEmpty(role))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Role, role),
            };

            var name = Context.Session.GetString("Name");
            if (!string.IsNullOrEmpty(name))
            {
                claims.Add(new Claim(ClaimTypes.Name, name));
            }

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}