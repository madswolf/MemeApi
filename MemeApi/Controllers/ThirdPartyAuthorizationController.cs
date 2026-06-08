using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using MemeApi.library;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services;
using MemeApi.Models.DTO;
using MemeApi.Models.DTO.ThirdParty;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MemeApi.Controllers;

/// <summary>
/// Issues and validates JWT tokens for trusted third-party services via the Discord bot.
/// The Discord bot authenticates with a shared secret and identifies the user; the API
/// issues a short-lived JWT and a long-lived refresh token the third-party service can
/// use to obtain new access tokens without involving the bot again.
/// </summary>
[ApiController]
[Route("auth")]
public class ThirdPartyAuthorizationController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly UserRepository _userRepository;
    private readonly RefreshTokenRepository _refreshTokenRepository;
    private readonly MemeApiSettings _settings;
    private readonly UserManager<User> _userManager;

    public ThirdPartyAuthorizationController(
        JwtTokenService jwtTokenService,
        UserRepository userRepository,
        RefreshTokenRepository refreshTokenRepository,
        MemeApiSettings settings,
        UserManager<User> userManager)
    {
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _settings = settings;
        _userManager = userManager;
    }

    /// <summary>
    /// Issues tokens. Supports two grant types:
    ///
    /// grant_type=discord_bot — The Discord bot authenticates with client_secret and provides
    /// the Discord user ID. Returns a 1-hour access token and a 14-day refresh token.
    ///
    /// grant_type=refresh_token — Exchange a valid refresh token for a new access token and
    /// rotated refresh token. The old refresh token is revoked immediately.
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDTO>> Token([FromForm] TokenRequestDTO request)
    {
        return request.grant_type switch
        {
            "discord_bot" => await HandleDiscordBotGrant(request),
            "refresh_token" => await HandleRefreshTokenGrant(request),
            _ => BadRequest(new { error = "unsupported_grant_type" }),
        };
    }

    /// <summary>
    /// Revokes a refresh token (RFC 7009). The token is immediately invalidated and cannot
    /// be used to obtain new access tokens. Already-issued access tokens remain valid until
    /// their 1-hour expiry.
    /// </summary>
    [HttpPost("revoke")]
    [AllowAnonymous]
    public async Task<IActionResult> Revoke([FromForm] string token)
    {
        var refreshToken = await _refreshTokenRepository.GetAsync(token);

        // Always return 200 per RFC 7009 — don't reveal whether a token existed.
        if (refreshToken == null || refreshToken.IsRevoked)
            return Ok();

        await _refreshTokenRepository.RevokeAsync(refreshToken);
        return Ok();
    }

    /// <summary>
    /// OpenID Connect discovery document. Third-party services can use this to discover
    /// the token and userinfo endpoints.
    /// </summary>
    [HttpGet("/.well-known/openid-configuration")]
    [AllowAnonymous]
    public ActionResult GetOpenIdConfiguration()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new
        {
            issuer = _settings.GetJwtIssuer(),
            token_endpoint = $"{baseUrl}/auth/token",
            revocation_endpoint = $"{baseUrl}/auth/revoke",
            introspection_endpoint = $"{baseUrl}/auth/introspect",
            userinfo_endpoint = $"{baseUrl}/auth/userinfo",
            jwks_uri = $"{baseUrl}/.well-known/jwks",
            grant_types_supported = new[] { "discord_bot", "refresh_token" },
            token_endpoint_auth_methods_supported = new[] { "client_secret_post" },
            id_token_signing_alg_values_supported = new[] { "HS256" },
            subject_types_supported = new[] { "public" },
            claims_supported = new[] { "sub", "preferred_username" },
        });
    }

    /// <summary>
    /// JSON Web Key Set endpoint. HS256 is a symmetric algorithm; third-party services
    /// cannot independently verify tokens without the signing secret. Use /auth/introspect
    /// to validate tokens instead.
    /// </summary>
    [HttpGet("/.well-known/jwks")]
    [AllowAnonymous]
    public ActionResult GetJwks()
    {
        return Ok(new
        {
            keys = new[] { new { kty = "oct", alg = "HS256", use = "sig" } },
        });
    }

    /// <summary>
    /// Token introspection per RFC 7662. Returns active status and claims for a token.
    /// Inactive or expired tokens return { "active": false }.
    /// </summary>
    [HttpPost("introspect")]
    [AllowAnonymous]
    public ActionResult<IntrospectionResponseDTO> Introspect([FromForm] string token)
    {
        var principal = _jwtTokenService.ValidateToken(token);

        if (principal == null)
            return Ok(new IntrospectionResponseDTO { Active = false });

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        return Ok(new IntrospectionResponseDTO
        {
            Active = true,
            Sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
            Username = principal.FindFirstValue("preferred_username"),
            Exp = new DateTimeOffset(jwtToken.ValidTo).ToUnixTimeSeconds(),
            Iss = _settings.GetJwtIssuer(),
            Aud = _settings.GetJwtAudience(),
        });
    }

    /// <summary>
    /// Returns the authenticated user's profile. Requires a valid Bearer token issued
    /// by the /auth/token endpoint.
    /// </summary>
    [HttpGet("userinfo")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UserInfoDTO>> UserInfo()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null) return Unauthorized();

        var user = await _userRepository.GetUser(userId);
        if (user == null) return NotFound();

        return Ok(user.ToUserInfo(_settings.GetMediaHost()));
    }

    private async Task<ActionResult<TokenResponseDTO>> HandleDiscordBotGrant(TokenRequestDTO request)
    {
        if (string.IsNullOrEmpty(request.client_secret) || string.IsNullOrEmpty(request.discord_user_id))
            return BadRequest(new { error = "invalid_request" });

        if (request.client_secret != _settings.GetBotSecret())
            return Unauthorized(new { error = "invalid_client" });

        var userId = request.discord_user_id.ExternalUserIdToGuid();
        var user = await _userRepository.GetUser(userId);

        if (user == null)
        {
            user = new User
            {
                Id = userId,
                UserName = request.discord_username ?? request.discord_user_id,
                ProfilePicFile = "default.jpg",
                LastLoginAt = DateTime.UtcNow,
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return StatusCode(500, new { error = "server_error" });
        }
        else
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return Ok(await IssueTokenPair(user));
    }

    private async Task<ActionResult<TokenResponseDTO>> HandleRefreshTokenGrant(TokenRequestDTO request)
    {
        if (string.IsNullOrEmpty(request.refresh_token))
            return BadRequest(new { error = "invalid_request" });

        var stored = await _refreshTokenRepository.GetAsync(request.refresh_token);

        if (stored == null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new { error = "invalid_grant" });

        // Rotate: revoke old token before issuing new pair.
        await _refreshTokenRepository.RevokeAsync(stored);

        return Ok(await IssueTokenPair(stored.User));
    }

    private async Task<TokenResponseDTO> IssueTokenPair(User user)
    {
        var (accessToken, expiresAt) = _jwtTokenService.GenerateToken(user);
        var refreshToken = await _refreshTokenRepository.CreateAsync(user.Id);

        return new TokenResponseDTO
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = (int)(expiresAt - DateTimeOffset.UtcNow).TotalSeconds,
            RefreshToken = refreshToken.Token,
        };
    }
}
