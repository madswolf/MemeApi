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
    private readonly TemporaryPasswordStore _temporaryPasswordStore;

    public ThirdPartyAuthorizationController(
        JwtTokenService jwtTokenService,
        UserRepository userRepository,
        RefreshTokenRepository refreshTokenRepository,
        MemeApiSettings settings,
        UserManager<User> userManager,
        TemporaryPasswordStore temporaryPasswordStore)
    {
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _settings = settings;
        _userManager = userManager;
        _temporaryPasswordStore = temporaryPasswordStore;
    }

    /// <summary>
    /// Initiates third-party authentication for a Discord user. The bot authenticates with
    /// client_secret and provides the Discord user ID; the API returns a short-lived temporary
    /// password (valid for 60 seconds) that the bot can display to the user. The temporary
    /// password is then exchanged for an access/refresh token pair via POST /auth/login.
    /// </summary>
    [HttpPost("initiate")]
    [AllowAnonymous]
    public async Task<ActionResult<InitiateAuthResponseDTO>> Initiate([FromForm] string client_secret, [FromForm] string discord_user_id, [FromForm] string? discord_username)
    {
        if (string.IsNullOrEmpty(client_secret) || string.IsNullOrEmpty(discord_user_id))
            return BadRequest(new { error = "invalid_request" });

        if (client_secret != _settings.GetBotSecret())
            return Unauthorized(new { error = "invalid_client" });

        var userId = discord_user_id.ExternalUserIdToGuid();
        var user = await _userRepository.GetUser(userId);

        if (user == null)
        {
            user = new User
            {
                Id = userId,
                UserName = discord_username ?? discord_user_id,
                ProfilePicFile = "default.jpg",
                LastLoginAt = DateTime.UtcNow,
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return StatusCode(500, new { error = "server_error" });
        }

        var temporaryPassword = _temporaryPasswordStore.Create(userId);

        return Ok(new InitiateAuthResponseDTO
        {
            temporary_password = temporaryPassword,
            expires_in = _temporaryPasswordStore.LifetimeInSeconds,
        });
    }

    /// <summary>
    /// Exchanges a temporary password (issued by POST /auth/initiate) for an access token
    /// and a 14-day refresh token. The temporary password is single-use and expires after 60 seconds.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDTO>> Login([FromForm] LoginRequestDTO request)
    {
        if (string.IsNullOrEmpty(request.temporary_password))
            return BadRequest(new { error = "invalid_request" });

        var userId = _temporaryPasswordStore.TryConsume(request.temporary_password);
        if (userId == null)
            return Unauthorized(new { error = "invalid_grant" });

        var user = await _userRepository.GetUser(userId);
        if (user == null)
            return Unauthorized(new { error = "invalid_grant" });

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Ok(await IssueTokenPair(user));
    }

    /// <summary>
    /// Exchanges a refresh token for a new access token and rotated refresh token.
    /// The old refresh token is revoked immediately.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDTO>> Refresh([FromForm] RefreshRequestDTO request)
    {
        if (string.IsNullOrEmpty(request.refresh_token))
            return BadRequest(new { error = "invalid_request" });

        var stored = await _refreshTokenRepository.GetAsync(request.refresh_token);

        if (stored == null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(new { error = "invalid_grant" });

        await _refreshTokenRepository.RevokeAsync(stored);

        return Ok(await IssueTokenPair(stored.User));
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
            initiate_endpoint = $"{baseUrl}/auth/initiate",
            login_endpoint = $"{baseUrl}/auth/login",
            refresh_endpoint = $"{baseUrl}/auth/refresh",
            revocation_endpoint = $"{baseUrl}/auth/revoke",
            introspection_endpoint = $"{baseUrl}/auth/introspect",
            userinfo_endpoint = $"{baseUrl}/auth/userinfo",
            jwks_uri = $"{baseUrl}/.well-known/jwks",
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
    /// by the /auth/login or /auth/refresh endpoint.
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
