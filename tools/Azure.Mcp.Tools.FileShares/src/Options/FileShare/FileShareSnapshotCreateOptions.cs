// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

public class FileShareSnapshotCreateOptions : BaseFileSharesOptions
{
    public string? FileShareName { get; set; }

    public string? SnapshotName { get; set; }

    public Dictionary<string, string>? Tags { get; set; }
}
