using System;
using System.Collections.Concurrent;

namespace MemeApi.library.Services;

public class TemporaryPasswordStore
{
    private const int LifetimeSeconds = 60;

    private readonly ConcurrentDictionary<string, (string UserId, string Scope, DateTime ExpiresAt)> _store = new();

    public string Create(string userId, string scope)
    {
        var password = Guid.NewGuid().ToString();
        _store[password] = (userId, scope, DateTime.UtcNow.AddSeconds(LifetimeSeconds));
        return password;
    }

    public (string UserId, string Scope)? TryConsume(string password)
    {
        if (!_store.TryRemove(password, out var entry))
            return null;

        if (entry.ExpiresAt < DateTime.UtcNow)
            return null;

        return (entry.UserId, entry.Scope);
    }

    public int LifetimeInSeconds => LifetimeSeconds;
}
