// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;

public class SnapshotGetOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.Ids)]
    public string[]? Ids { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Pool)]
    public string? Pool { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Volume)]
    public string? Volume { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Snapshot)]
    public string? Snapshot { get; set; }
}
