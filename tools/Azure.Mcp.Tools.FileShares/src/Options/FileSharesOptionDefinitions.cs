// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.FileShares.Options;

public static class FileSharesOptionDefinitions
{
    public const string FileShareName = "file-share";
    public const string LocationName = "location";
    public const string SnapshotIdName = "snapshot-id";
    public const string ConnectionNameName = "connection-name";
    public const string StatusName = "status";
    public const string FilterName = "filter";

    public static readonly Option<string> FileShare = new(
        $"--{FileShareName}"
    )
    {
        Description = "The name of the file share.",
        Required = true
    };

    public static readonly Option<string> Location = new(
        $"--{LocationName}"
    )
    {
        Description = "The Azure region for the resource.",
        Required = true
    };

    public static readonly Option<string> SnapshotId = new(
        $"--{SnapshotIdName}"
    )
    {
        Description = "The ID of the file share snapshot.",
        Required = true
    };

    public static readonly Option<string> ConnectionName = new(
        $"--{ConnectionNameName}"
    )
    {
        Description = "The name of the private endpoint connection.",
        Required = true
    };

    public static readonly Option<string> Status = new(
        $"--{StatusName}"
    )
    {
        Description = "The approval status (Approved, Rejected, or Removed).",
        Required = true
    };

    public static readonly Option<string> Filter = new(
        $"--{FilterName}"
    )
    {
        Description = "Filter expression for querying resources.",
        Required = false
    };
}
