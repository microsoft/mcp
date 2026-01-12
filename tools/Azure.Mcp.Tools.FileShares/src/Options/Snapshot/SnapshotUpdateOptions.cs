// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.Snapshot;

public class SnapshotUpdateOptions : BaseFileSharesOptions
{
    public string? FileShareName { get; set; }
    public string? SnapshotName { get; set; }
    public string? Metadata { get; set; }
}
