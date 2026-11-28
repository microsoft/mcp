// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ApiRequestInvokeOptions()
{
    [Option(Description = "The REST API to call: fabric (https://api.fabric.microsoft.com) or powerbi (https://api.powerbi.com). Defaults to fabric. Selects both the host and the token scope; other hosts are not allowed.")]
    public string? Api { get; set; }

    [Option(Description = "The relative REST path, with an optional query string. Must start with /v1/ for fabric or /v1.0/myorg/ for powerbi, for example /v1/workspaces/{workspaceId}/items/{itemId}/getDefinition or /v1.0/myorg/groups/{groupId}/datasets/{datasetId}/refreshes. Absolute URLs, fragments, whitespace, empty or dot segments, and encoded path separators are rejected.")]
    public required string Path { get; set; }

    [Option(Description = "The HTTP method: POST, PUT, PATCH, or DELETE. GET is not accepted; use core_get-api-resource to read resources.")]
    public required string Method { get; set; }

    [Option(Description = "Optional request body as a JSON object or array, up to 1 MiB (1048576 UTF-8 bytes) and 32 nesting levels. Sent unchanged as application/json. Omit for requests without a body.")]
    public string? Body { get; set; }

    [Option(Description = "Maximum JSON response size in bytes. Defaults to 1048576 (1 MiB); maximum 16777216 (16 MiB). Larger responses fail instead of being truncated.")]
    public int? MaxResponseBytes { get; set; }

    [Option(Description = "For api fabric only: wait asynchronously for a 202 long-running operation to complete. Defaults to false, returning an operation receipt without polling. Not supported for api powerbi.", DefaultValue = false)]
    public bool Sync { get; set; }

    [Option(Description = "Finite positive local wait budget in seconds after acceptance when sync is true. Defaults to 120; maximum 600 (10 minutes). Expiry returns a resumable receipt and does not cancel Fabric's operation.", DefaultValue = FabricOperationWaitOptions.DefaultMaxWaitSeconds)]
    public double MaxWaitSeconds { get; set; } = FabricOperationWaitOptions.DefaultMaxWaitSeconds;

    [Option(Description = "Allow one early status probe after at most 3 seconds, even if the initial 202 Retry-After is longer. Defaults to true; false honors the initial hint.", DefaultValue = true)]
    public bool EarlyPoll { get; set; } = true;

    [Option(Description = "When true, also return the operation's JSON result after a waited Fabric operation succeeds. Requires sync true. Use only for operations that document a result, such as getDefinition.", DefaultValue = false)]
    public bool OperationResult { get; set; }
}
