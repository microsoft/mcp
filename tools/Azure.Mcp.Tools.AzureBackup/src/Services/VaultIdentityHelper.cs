// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Shared helpers for translating the vault-update managed identity options
/// (<c>--identity-type</c> and <c>--user-assigned-identity</c>) into a
/// <see cref="ManagedServiceIdentity"/> for Recovery Services and Backup vaults.
/// </summary>
internal static class VaultIdentityHelper
{
    public static ManagedServiceIdentityType ParseIdentityType(string identityType) =>
        identityType.ToUpperInvariant() switch
        {
            "SYSTEMASSIGNED" => ManagedServiceIdentityType.SystemAssigned,
            "USERASSIGNED" => ManagedServiceIdentityType.UserAssigned,
            "SYSTEMASSIGNED,USERASSIGNED" or "SYSTEMASSIGNEDUSERASSIGNED"
                => ManagedServiceIdentityType.SystemAssignedUserAssigned,
            "NONE" => ManagedServiceIdentityType.None,
            _ => throw new ArgumentException(
                $"Invalid identity type '{identityType}'. Supported values: 'SystemAssigned', 'UserAssigned', 'SystemAssigned,UserAssigned', 'None'.")
        };

    /// <summary>
    /// Builds a <see cref="ManagedServiceIdentity"/> from the requested identity type and an
    /// optional comma-separated list of user-assigned identity ARM resource IDs. When the type
    /// includes <c>UserAssigned</c>, at least one identity ID is required; when it does not, no
    /// IDs may be supplied.
    /// </summary>
    public static ManagedServiceIdentity BuildManagedServiceIdentity(string identityType, string? userAssignedIdentity)
    {
        var type = ParseIdentityType(identityType);
        var identity = new ManagedServiceIdentity(type);

        var ids = ParseUserAssignedIdentityIds(userAssignedIdentity);
        var includesUserAssigned =
            type == ManagedServiceIdentityType.UserAssigned ||
            type == ManagedServiceIdentityType.SystemAssignedUserAssigned;

        if (includesUserAssigned)
        {
            if (ids.Count == 0)
            {
                throw new ArgumentException(
                    "--user-assigned-identity is required when --identity-type includes 'UserAssigned'. Provide one or more user-assigned managed identity ARM resource IDs (comma-separated).");
            }

            foreach (var id in ids)
            {
                identity.UserAssignedIdentities[new ResourceIdentifier(id)] = new UserAssignedIdentity();
            }
        }
        else if (ids.Count > 0)
        {
            throw new ArgumentException(
                "--user-assigned-identity can only be used when --identity-type includes 'UserAssigned' (i.e., 'UserAssigned' or 'SystemAssigned,UserAssigned').");
        }

        return identity;
    }

    private static List<string> ParseUserAssignedIdentityIds(string? userAssignedIdentity)
    {
        if (string.IsNullOrWhiteSpace(userAssignedIdentity))
        {
            return [];
        }

        var ids = new List<string>();
        foreach (var raw in userAssignedIdentity.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            ResourceIdentifier parsed;
            try
            {
                parsed = new ResourceIdentifier(raw);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                throw new ArgumentException(
                    $"Invalid --user-assigned-identity value '{raw}'. Expected an ARM resource ID of the form '/subscriptions/{{sub}}/resourceGroups/{{rg}}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{{name}}'.", ex);
            }

            if (parsed.ResourceType != "Microsoft.ManagedIdentity/userAssignedIdentities")
            {
                throw new ArgumentException(
                    $"Invalid --user-assigned-identity value '{raw}': resource type '{parsed.ResourceType}' is not 'Microsoft.ManagedIdentity/userAssignedIdentities'.");
            }

            ids.Add(parsed.ToString());
        }

        return ids;
    }
}
