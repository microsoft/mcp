// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace Azure.Mcp.Tools.InfraIq.Services;

/// <summary>
/// Remembers, per request route, that the global ARM endpoint reported the action as unavailable so later calls go
/// straight to the Canary endpoint. Entries expire so the global endpoint is retried and used once it comes online.
/// Holds no caller credentials or response data.
/// </summary>
internal sealed class InfraIqIngressFallbackCache(TimeProvider timeProvider)
{
    public static readonly TimeSpan EntryLifetime = TimeSpan.FromMinutes(10);

    private const int MaxEntries = 1024;

    private readonly ConcurrentDictionary<string, DateTimeOffset> _expirations = new(StringComparer.OrdinalIgnoreCase);

    public bool IsPrimaryUnavailable(string routeKey)
    {
        if (!_expirations.TryGetValue(routeKey, out var expiresAt))
        {
            return false;
        }

        if (timeProvider.GetUtcNow() < expiresAt)
        {
            return true;
        }

        _expirations.TryRemove(new KeyValuePair<string, DateTimeOffset>(routeKey, expiresAt));
        return false;
    }

    public void MarkPrimaryUnavailable(string routeKey)
    {
        var now = timeProvider.GetUtcNow();
        if (_expirations.Count >= MaxEntries)
        {
            foreach (var entry in _expirations)
            {
                if (entry.Value <= now)
                {
                    _expirations.TryRemove(entry);
                }
            }

            if (_expirations.Count >= MaxEntries)
            {
                _expirations.Clear();
            }
        }

        _expirations[routeKey] = now + EntryLifetime;
    }
}
