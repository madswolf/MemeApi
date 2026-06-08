using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MemeApi.Models.Entity;
using Microsoft.IdentityModel.Tokens;

namespace MemeApi.library.Services;

public class JwtTokenService
{
    public const string ScopeTransferDubloons = "transfer_dubloons";
    public const string ScopeSubmitPlace = "submit_place";

    private const int TokenLifetimeHours = 1;
    private readonly MemeApiSettings _settings;

    public JwtTokenService(MemeApiSettings settings)
    {
        _settings = settings;
    }

    public (string token, DateTimeOffset expiresAt) GenerateToken(User user, string scope)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.GetJwtSecret()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(TokenLifetimeHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("preferred_username", user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("scope", scope),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.GetJwtIssuer(),
            audience: _settings.GetJwtAudience(),
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.GetJwtSecret()));

        try
        {
            return tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _settings.GetJwtIssuer(),
                ValidateAudience = true,
                ValidAudience = _settings.GetJwtAudience(),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
        }
        catch
        {
            return null;
        }
    }
}
