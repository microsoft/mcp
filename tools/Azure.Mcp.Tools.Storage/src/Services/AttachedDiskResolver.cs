// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Storage.Services;

public static class AttachedDiskResolver
{
    public static string[] ResolveResourceIds(
        IEnumerable<(string? Name, string? ResourceId)> attachedDisks,
        IEnumerable<string> requestedDiskNames)
    {
        var requestedNames = requestedDiskNames
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var attachedByName = attachedDisks
            .Where(disk => !string.IsNullOrWhiteSpace(disk.Name))
            .ToDictionary(disk => disk.Name!, disk => disk.ResourceId, StringComparer.OrdinalIgnoreCase);
        var missingNames = requestedNames
            .Where(name => !attachedByName.TryGetValue(name, out var resourceId) || string.IsNullOrWhiteSpace(resourceId))
            .ToList();
        if (missingNames.Count > 0)
        {
            throw new ArgumentException(
                $"The following disks are not attached to the virtual machine: {string.Join(", ", missingNames)}.",
                nameof(requestedDiskNames));
        }

        return requestedNames.Select(name => attachedByName[name]!).ToArray();
    }
}
