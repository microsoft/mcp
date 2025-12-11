// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.FileShare;

public class FileShareSnapshotListOptions : BaseFileSharesOptions
{
    public string? FileShareName { get; set; }

    public string? Filter { get; set; }
}
