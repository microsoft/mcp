// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Resolves a user-supplied RSV protected item (and optional container hint) to a single listed item (UX-1).
/// </summary>
/// <remarks>
/// Agents often pass friendly names, such as a VM name, where RSV expects semicolon-delimited internal names
/// (container <c>IaasVMContainer;iaasvmcontainerv2;{rg};{vm}</c>, item <c>VM;iaasvmcontainerv2;{rg};{vm}</c>).
/// The service rejects those with <c>BMSUserErrorContainerNameIncorrectFormat</c> or
/// <c>BMSUserErrorDataSourceObjectNotFound</c>. This matcher maps them to the listed item instead.
/// </remarks>
internal static class RsvProtectedItemMatcher
{
    private const int MaxCandidatesInMessage = 10;
    private const string ContainersSegment = "/protectionContainers/";

    private static readonly HashSet<string> s_itemLookupErrorCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BMSUserErrorContainerNameIncorrectFormat",
        "BMSUserErrorProtectedItemNameIncorrectFormat",
        "BMSUserErrorDataSourceObjectNotFound",
    };

    /// <summary>
    /// Returns true when <paramref name="value"/> is a semicolon-delimited RSV internal name.
    /// </summary>
    internal static bool IsQualifiedName(string? value) => !string.IsNullOrWhiteSpace(value) && value.Contains(';');

    /// <summary>
    /// Returns true when the container and item must be resolved from the protected item list before calling the service.
    /// </summary>
    internal static bool RequiresResolution(string protectedItem, string? container) =>
        !IsQualifiedName(container) || !IsQualifiedName(protectedItem);

    /// <summary>
    /// Returns true when a service error indicates the container or item name did not identify an item.
    /// </summary>
    internal static bool IsItemLookupFailure(RequestFailedException ex) =>
        ex.ErrorCode is not null && s_itemLookupErrorCodes.Contains(ex.ErrorCode);

    /// <summary>
    /// Finds listed items matching <paramref name="protectedItem"/> and, if provided, <paramref name="containerHint"/>.
    /// Exact internal-name matches take precedence over friendly-name matches.
    /// </summary>
    internal static List<ProtectedItemInfo> FindCandidates(
        IEnumerable<ProtectedItemInfo> items, string protectedItem, string? containerHint)
    {
        ArgumentNullException.ThrowIfNull(items);
        var itemValue = protectedItem.Trim();
        var hint = string.IsNullOrWhiteSpace(containerHint) ? null : containerHint.Trim();

        var matches = items
            .Where(i => MatchesItem(i, itemValue) && (hint is null || MatchesContainer(i, hint)))
            .ToList();

        var exact = matches.Where(i => string.Equals(i.Name, itemValue, StringComparison.OrdinalIgnoreCase)).ToList();
        if (exact.Count > 0)
        {
            matches = exact;
        }

        return matches
            .DistinctBy(i => $"{GetContainerName(i)}|{i.Name}", StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Resolves exactly one listed item that has a known container.
    /// </summary>
    /// <exception cref="KeyNotFoundException">No listed item matches.</exception>
    /// <exception cref="CommandValidationException">More than one item matches, or the match has no container.</exception>
    internal static ProtectedItemInfo Resolve(
        IEnumerable<ProtectedItemInfo> items, string protectedItem, string? containerHint, string vaultName)
    {
        var candidates = FindCandidates(items, protectedItem, containerHint);
        var withHint = string.IsNullOrWhiteSpace(containerHint) ? string.Empty : $" with a container matching '{containerHint}'";

        if (candidates.Count == 0)
        {
            throw new KeyNotFoundException(
                $"Protected item '{protectedItem}' was not found in vault '{vaultName}'{withHint}. " +
                "Run 'azurebackup protecteditem get' without --protected-item to list the protected items and their containers.");
        }

        if (candidates.Count > 1)
        {
            var listed = string.Join("; ", candidates
                .Take(MaxCandidatesInMessage)
                .Select(c => $"--protected-item '{c.Name}' --container '{GetContainerName(c)}'"));
            throw new CommandValidationException(
                $"Protected item '{protectedItem}' matches {candidates.Count} items in vault '{vaultName}'{withHint}. " +
                $"Specify one of: {listed}.")
            {
                TelemetrySafeMessage = "Protected item name matches multiple items."
            };
        }

        var match = candidates[0];
        if (GetContainerName(match) is null)
        {
            throw new CommandValidationException(
                $"Could not determine the container for protected item '{match.Name}' in vault '{vaultName}'. " +
                "Provide --container with the full container name (for example, 'IaasVMContainer;iaasvmcontainerv2;{resource-group};{vm-name}').")
            {
                TelemetrySafeMessage = "Protected item container could not be determined."
            };
        }

        return match;
    }

    /// <summary>
    /// Returns the full container name of a listed item. Listings report IaaS VM containers in a short
    /// form ('iaasvmcontainerv2;{rg};{vm}') that item-scoped APIs such as recovery points reject, so the
    /// container segment of the item's resource ID ('IaasVMContainer;iaasvmcontainerv2;{rg};{vm}') is preferred.
    /// </summary>
    internal static string? GetContainerName(ProtectedItemInfo item) =>
        GetContainerNameFromId(item.Id) ?? (string.IsNullOrWhiteSpace(item.ContainerName) ? null : item.ContainerName);

    private static string? GetContainerNameFromId(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var start = id.IndexOf(ContainersSegment, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return null;
        }

        start += ContainersSegment.Length;
        var end = id.IndexOf('/', start);
        var name = end < 0 ? id[start..] : id[start..end];
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private static bool MatchesItem(ProtectedItemInfo item, string value)
    {
        if (EqualsOrdinalIgnoreCase(item.Name, value) ||
            EqualsOrdinalIgnoreCase(LastSegment(item.Name, ';'), value) ||
            EqualsOrdinalIgnoreCase(item.Details?.FriendlyName, value))
        {
            return true;
        }

        return !string.IsNullOrEmpty(item.DatasourceId) &&
            EqualsOrdinalIgnoreCase(LastSegment(item.DatasourceId.TrimEnd('/'), '/'), value);
    }

    private static bool MatchesContainer(ProtectedItemInfo item, string hint)
    {
        foreach (var container in new[] { item.ContainerName, GetContainerNameFromId(item.Id) })
        {
            if (string.IsNullOrWhiteSpace(container))
            {
                continue;
            }

            // Accept the full name, the short form returned in listings, a trailing part such as
            // '{rg};{vm}', or just the VM/storage account name.
            if (EqualsOrdinalIgnoreCase(container, hint) ||
                container.EndsWith(";" + hint, StringComparison.OrdinalIgnoreCase) ||
                hint.EndsWith(";" + container, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string? LastSegment(string? value, char separator) =>
        string.IsNullOrEmpty(value) ? null : value.Split(separator)[^1];

    private static bool EqualsOrdinalIgnoreCase(string? left, string right) =>
        !string.IsNullOrEmpty(left) && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
