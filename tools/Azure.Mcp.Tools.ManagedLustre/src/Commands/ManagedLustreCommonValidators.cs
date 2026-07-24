// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ManagedLustre.Commands;

internal static class ManagedLustreCommonValidators
{
    internal static void ValidateRootSquashOptions(ValidationResult validationResult, string? rootSquashMode, string? noSquashNidLists, long? squashUid, long? squashGid)
    {
        // If root squash mode is provided and not 'none', require UID, GID and no squash NID list
        if (!string.IsNullOrWhiteSpace(rootSquashMode) && !rootSquashMode.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            if (!(squashUid.HasValue && squashGid.HasValue && !string.IsNullOrWhiteSpace(noSquashNidLists)))
            {
                validationResult.Errors.Add("When --root-squash-mode is not 'None', --squash-uid, --squash-gid and --no-squash-nid-list must be provided.");
            }
        }
    }
}
