using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MemeApi.library.Authentication;

public class BotSecretAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "BotSecret";

    private readonly MemeApiSettings _settings;

    public BotSecretAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        MemeApiSettings settings) : base(options, logger, encoder)
    {
        _settings = settings;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Bot_Secret", out var botSecret) ||
            botSecret != _settings.GetBotSecret())
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim> { new Claim("client_type", "discord_bot") };

        if (Request.Headers.TryGetValue("ExternalUserId", out var externalUserId))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, externalUserId.ToString().ExternalUserIdToGuid()));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
