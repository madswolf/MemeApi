using System;
using System.Linq;
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

    public async Task<RefreshToken> CreateAsync(string userId, string scope)
    {
        var token = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = userId,
            Scope = scope,
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

    public async Task<int> RevokeAllForUserAsync(string userId)
    {
        var active = await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var t in active)
            t.IsRevoked = true;

        await _context.SaveChangesAsync();
        return active.Count;
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
