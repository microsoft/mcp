// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public class ItemDefinitionGetOptions
{
    [Option(Description = "The nonempty UUID of the Fabric workspace containing the item. Required with item-id for an initial request; cannot be combined with operation-id. Names are not resolved.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "The nonempty UUID of the Fabric item whose definition is requested. Required with workspace-id for an initial request; cannot be combined with operation-id. Names are not resolved.")]
    public string? ItemId { get; set; }

    [Option(Description = "Optional definition format, such as ipynb or FabricGitSource. Omit to use the item's default. Must not be empty or whitespace, must be at most 128 characters, and contain no control characters. Passed as one encoded query value; Fabric validates support. Cannot be combined with operation-id.")]
    public string? Format { get; set; }

    [Option(Description = "Optional new absolute local directory for the completed definition's decoded files. Its parent must exist. Never overwrites or merges a destination. Local execution only; unavailable over HTTP. Omit to return only inline definition content without writing files.")]
    public string? OutputDirectory { get; set; }

    [Option(Description = "Nonempty UUID from an earlier Get Item Definition receipt. After get-operation-state reports Succeeded, retrieve this definition result once with one GET and no POST or polling. The result does not identify its item or prove its provenance. Cannot be combined with workspace-id, item-id, format, or sync=true.")]
    public string? OperationId { get; set; }

    [Option(Description = "Wait for accepted definition retrieval, poll its state, and retrieve its result within max-wait-seconds. Defaults to false, returning an operation receipt without polling. Cannot be combined with operation-id.", DefaultValue = false)]
    public bool Sync { get; set; }

    [Option(Description = "Finite positive local wait budget after acceptance when sync=true. Defaults to 120 seconds; maximum 600 seconds. Fractional values are allowed. Expiry returns the operation ID without saving files or canceling the Fabric operation.", DefaultValue = FabricOperationWaitOptions.DefaultMaxWaitSeconds)]
    public double MaxWaitSeconds { get; set; } = FabricOperationWaitOptions.DefaultMaxWaitSeconds;

    [Option(Description = "Allow one initial status probe after at most 3 seconds, even when the initial Retry-After is longer. Defaults to true. False honors the initial hint; subsequent polling and 429/503 backoff honor validated hints.", DefaultValue = true)]
    public bool EarlyPoll { get; set; } = true;
}
