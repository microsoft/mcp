// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.Snapshot;

public class SnapshotDeleteOptions : BaseFileSharesOptions
{
    public string? FileShareName { get; set; }
    public string? SnapshotName { get; set; }
}
