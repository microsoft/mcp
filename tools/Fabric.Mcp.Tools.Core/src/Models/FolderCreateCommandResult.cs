// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FolderCreateCommandResult(
    FabricFolder Folder,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string CreationStatus => "Created";

    public string Guidance =>
        "Fabric confirmed folder creation (HTTP 201). No lookup, follow-up read, polling, or automatic retry was performed. Do not resubmit this creation.";
}
