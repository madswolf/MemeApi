using System;
using System.Collections.Concurrent;

namespace MemeApi.library.Services;

public class TemporaryPasswordStore
{
    private const int LifetimeSeconds = 60;

    private readonly ConcurrentDictionary<string, (string UserId, DateTime ExpiresAt)> _store = new();

    public string Create(string userId)
    {
        var password = Guid.NewGuid().ToString();
        _store[password] = (userId, DateTime.UtcNow.AddSeconds(LifetimeSeconds));
        return password;
    }

    public string? TryConsume(string password)
    {
        if (!_store.TryRemove(password, out var entry))
            return null;

        if (entry.ExpiresAt < DateTime.UtcNow)
            return null;

        return entry.UserId;
    }

    public int LifetimeInSeconds => LifetimeSeconds;
}
