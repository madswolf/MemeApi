using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MemeApi.Models.Context;
using MemeApi.Models.Entity.ThirdParty;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories;

public class RefreshTokenRepository
{
    private const int TokenLifetimeDays = 14;
    private readonly MemeContext _context;

    public RefreshTokenRepository(MemeContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken> CreateAsync(string userId)
    {
        var token = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(TokenLifetimeDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
        };
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<RefreshToken?> GetAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);
    }

    public async Task RevokeAsync(RefreshToken refreshToken)
    {
        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
