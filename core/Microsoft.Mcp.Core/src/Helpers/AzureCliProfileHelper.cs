// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Reads the default Azure subscription from the Azure CLI profile
/// stored at ~/.azure/azureProfile.json (set via 'az account set').
/// </summary>
public static class AzureCliProfileHelper
{
    /// <summary>
    /// Gets the default subscription ID from the Azure CLI profile (~/.azure/azureProfile.json).
    /// </summary>
    /// <returns>The default subscription ID if found, null otherwise.</returns>
    public static string? GetDefaultSubscriptionId()
    {
        try
        {
            var profilePath = GetAzureProfilePath();
            if (string.IsNullOrEmpty(profilePath) || !File.Exists(profilePath))
            {
                return null;
            }

            // Synchronous read is intentional: the Azure CLI profile is a small local file
            // and the result is cached by CommandHelper so this runs at most once per process.
            var json = File.ReadAllText(profilePath);
            return ParseDefaultSubscriptionId(json);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or SecurityException)
        {
            // Best-effort: profile may be missing, corrupted, or inaccessible
            return null;
        }
    }

    /// <summary>
    /// Parses the default subscription ID from the given Azure CLI profile JSON content.
    /// </summary>
    /// <param name="json">The JSON content of the Azure CLI profile.</param>
    /// <returns>The default subscription ID if found, null otherwise.</returns>
    internal static string? ParseDefaultSubscriptionId(string json)
    {
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("subscriptions", out var subscriptions) ||
            subscriptions.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var sub in subscriptions.EnumerateArray())
        {
            if (sub.TryGetProperty("isDefault", out var isDefault) &&
                isDefault.ValueKind == JsonValueKind.True &&
                sub.TryGetProperty("id", out var id) &&
                id.ValueKind == JsonValueKind.String)
            {
                return id.GetString();
            }
        }

        return null;
    }

    internal static string? GetAzureProfilePath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(userProfile))
        {
            return null;
        }

        var azureDir = Path.Combine(userProfile, ".azure");
        return Path.Combine(azureDir, "azureProfile.json");
    }
}
