// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Optional 'vault get' expansion flags. Selects extra vault posture fields
/// (encryption / cross-region-restore state, MUA state) that are off by
/// default to preserve the legacy response shape and avoid extra Resource
/// Guard API calls for callers that don't need them.
/// </summary>
[Flags]
public enum VaultExpand
{
    None = 0,
    Security = 1 << 0,
    Mua = 1 << 1,
    All = Security | Mua,
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
                "mua" => VaultExpand.Mua,
                "all" => VaultExpand.All,
                _ => throw new ArgumentException(
                    $"Invalid --expand value '{raw}'. Supported values: 'security', 'mua', 'all' (comma-separated).")
            };
        }

        return result;
    }
}
