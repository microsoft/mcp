// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

public class FileShareSnapshotGetOptions : BaseFileSharesOptions
{
    public string? FileShareName { get; set; }

    public string? SnapshotName { get; set; }
}
