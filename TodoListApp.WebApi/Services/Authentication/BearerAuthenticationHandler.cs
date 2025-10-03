using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TodoListApp.WebApi.Services.Authentication;

/// <summary>
/// Custom authentication handler that validates a static Bearer token from configuration.
/// </summary>
/// <remarks>
/// This handler reads the "Authorization" header of incoming HTTP requests and checks for
/// a valid "Bearer" token that matches the configured value in <c>appsettings.json</c>.
/// If the token matches, authentication succeeds; otherwise, the request is rejected.
/// </remarks>
public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration config;

    public BearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration config)
        : base(options, logger, encoder, clock)
    {
        this.config = config;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!this.Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
        }

        var authHeader = this.Request.Headers["Authorization"].ToString();

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid scheme."));
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var validToken = this.config["Authentication:BearerToken"];

        if (token != validToken)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token."));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "AuthorizedUser") };
        var identity = new ClaimsIdentity(claims, this.Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
