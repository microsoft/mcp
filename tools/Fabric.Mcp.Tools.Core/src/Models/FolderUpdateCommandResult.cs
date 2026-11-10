// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FolderUpdateCommandResult(
    FabricFolder Folder,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string UpdateStatus => "Updated";

    public string Guidance =>
        "Fabric confirmed folder update (HTTP 200). This is the returned metadata snapshot. No lookup, follow-up read, polling, or automatic retry was performed.";
}
