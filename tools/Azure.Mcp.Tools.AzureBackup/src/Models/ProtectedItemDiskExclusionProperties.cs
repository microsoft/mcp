// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemDiskExclusionProperties(
    IReadOnlyList<int>? DiskLunList,
    bool? IsInclusionList);
