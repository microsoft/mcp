// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;

/// <summary>
/// Shared parsing and validation for the selective disk backup options
/// (<c>--disk-list-setting</c>, <c>--disks-list</c>, <c>--exclude-all-data-disks</c>)
/// used by <c>protecteditem protect</c> and <c>protecteditem update-protection</c>.
/// </summary>
internal static class DiskExclusionValidator
{
    private static readonly HashSet<string> AllowedSettings = new(StringComparer.OrdinalIgnoreCase)
    {
        DiskExclusionSpec.SettingInclude,
        DiskExclusionSpec.SettingExclude,
        DiskExclusionSpec.SettingReset,
    };

    /// <summary>
    /// Validates the combination of selective disk options at command-binding time
    /// (transport-agnostic, no Azure calls). Adds messages to <paramref name="validationResult"/>
    /// on failure.
    /// </summary>
    public static void ValidateDiskExclusionOptions(
        string? diskListSetting,
        string? disksList,
        bool excludeAllDataDisks,
        ValidationResult validationResult)
    {
        var hasSetting = !string.IsNullOrWhiteSpace(diskListSetting);
        var hasList = !string.IsNullOrWhiteSpace(disksList);

        if (hasSetting)
        {
            var normalized = diskListSetting!.Trim();
            if (!AllowedSettings.Contains(normalized))
            {
                validationResult.Errors.Add(
                    $"Invalid --disk-list-setting value '{diskListSetting}'. " +
                    $"Allowed values: {string.Join(", ", AllowedSettings)}.");
                return;
            }

            var isReset = string.Equals(normalized, DiskExclusionSpec.SettingReset, StringComparison.OrdinalIgnoreCase);
            if (isReset)
            {
                if (hasList || excludeAllDataDisks)
                {
                    validationResult.Errors.Add(
                        "--disks-list and --exclude-all-data-disks cannot be combined with --disk-list-setting 'resetexclusionsettings'.");
                }
                return;
            }

            // include / exclude require either --disks-list or --exclude-all-data-disks.
            if (!hasList && !excludeAllDataDisks)
            {
                validationResult.Errors.Add(
                    "When --disk-list-setting is 'include' or 'exclude', either --disks-list or --exclude-all-data-disks must be provided.");
            }

            if (hasList && excludeAllDataDisks)
            {
                validationResult.Errors.Add(
                    "--disks-list and --exclude-all-data-disks are mutually exclusive.");
            }
        }
        else
        {
            // --disks-list without --disk-list-setting is ambiguous.
            if (hasList)
            {
                validationResult.Errors.Add(
                    "--disks-list requires --disk-list-setting to be set to 'include' or 'exclude'.");
            }

            if (excludeAllDataDisks && !hasSetting)
            {
                // exclude-all-data-disks by itself is a valid shortcut for
                // "back up only the OS disk" - no explicit setting required.
            }
        }

        if (hasList)
        {
            foreach (var raw in disksList!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!int.TryParse(raw, out var lun) || lun < 0)
                {
                    validationResult.Errors.Add(
                        $"Invalid disk LUN '{raw}' in --disks-list. LUNs must be non-negative integers (e.g. '0,1,3').");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Builds a <see cref="DiskExclusionSpec"/> from the raw option values, or returns
    /// <see langword="null"/> when none of the disk options were provided.
    /// </summary>
    public static DiskExclusionSpec? BuildDiskExclusionSpec(
        string? diskListSetting,
        string? disksList,
        bool excludeAllDataDisks)
    {
        var spec = new DiskExclusionSpec(diskListSetting, disksList, excludeAllDataDisks);
        return spec.HasAnyValue ? spec : null;
    }
}
