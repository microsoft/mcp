// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Optional 'vault get' expansion flags. Selects extra vault posture fields
/// (security settings, network state, monitoring alerts, MUA state) that are
/// off by default to preserve the legacy response shape and avoid extra API
/// calls for callers that don't need them.
/// </summary>
[Flags]
public enum VaultExpand
{
    None = 0,
    Security = 1 << 0,
    Network = 1 << 1,
    Monitoring = 1 << 2,
    Mua = 1 << 3,
    All = Security | Network | Monitoring | Mua,
}

public static class VaultExpandParser
{
    /// <summary>
    /// Parses a CSV expand string (e.g. "security,network") into <see cref="VaultExpand"/>.
    /// Values are case-insensitive. Whitespace between tokens is ignored. Empty/null input
    /// returns <see cref="VaultExpand.None"/>. Unknown tokens throw <see cref="ArgumentException"/>.
    /// </summary>
    public static VaultExpand Parse(string? expand)
    {
        if (string.IsNullOrWhiteSpace(expand))
        {
            return VaultExpand.None;
        }

        var result = VaultExpand.None;
        foreach (var raw in expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            result |= raw.ToLowerInvariant() switch
            {
                "security" => VaultExpand.Security,
                "network" => VaultExpand.Network,
                "monitoring" => VaultExpand.Monitoring,
                "mua" => VaultExpand.Mua,
                "all" => VaultExpand.All,
                _ => throw new ArgumentException(
                    $"Invalid --expand value '{raw}'. Supported values: 'security', 'network', 'monitoring', 'mua', 'all' (comma-separated).")
            };
        }

        return result;
    }
}
