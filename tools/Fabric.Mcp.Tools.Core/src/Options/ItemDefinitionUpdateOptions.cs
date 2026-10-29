// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemDefinitionUpdateOptions()
{
    [Option(Description = "The nonempty UUID of the existing Fabric workspace. Workspace names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The nonempty UUID of the existing Fabric item whose entire definition will be replaced. Item names are not resolved.")]
    public required string ItemId { get; set; }

    [OptionContainer<DefinitionInputOptions>(Prefix = "")]
    public DefinitionInputOptions? DefinitionInput { get; set; }

    [Option(Description = "Optional local structure-validation hint, not the API target identity. Notebook, DataPipeline, SemanticModel, and Report enable existing minimum checks for recognized/default formats. Omit for generic checks; other types and explicit unrecognized formats also receive generic checks. Never sent to Fabric, inferred from .platform, or resolved by a metadata lookup. Fabric validates the actual item's requirements.")]
    public string? DefinitionItemType { get; set; }

    [Option(Description = "The API's top-level item-specific options as a JSON object string, up to 1 MiB (1048576 UTF-8 bytes) and 32 nesting levels, with no duplicate properties. Consult the actual item type's Update Definition API for supported options. Not LRO wait settings; use sync, max-wait-seconds, and early-poll for waiting.")]
    public string? Options { get; set; }

    [Option(Description = "Explicit true or false. Only true requests metadata updates from .platform when that part is supplied. Omit to leave the API query parameter omitted; false is sent explicitly. The tool never adds, removes, or rewrites .platform and this API never changes the item's sensitivity label.")]
    public bool? UpdateMetadata { get; set; }

    [Option(Description = "Wait asynchronously for accepted definition replacement to complete. Defaults to false, returning an operation receipt without polling. This is a no-result API: only operation state is polled, never /result.", DefaultValue = false)]
    public bool Sync { get; set; }

    [Option(Description = "Finite positive local wait budget in seconds after acceptance when sync is true. Defaults to 120; maximum 600 (10 minutes). Fractional seconds are allowed. Expiry returns a resumable receipt and does not cancel Fabric's operation.", DefaultValue = FabricOperationWaitOptions.DefaultMaxWaitSeconds)]
    public double MaxWaitSeconds { get; set; } = FabricOperationWaitOptions.DefaultMaxWaitSeconds;

    [Option(Description = "Allow one early status probe after at most 3 seconds, even if the initial 202 Retry-After is longer. Defaults to true; false honors the initial hint. Later polls and 429/503 retries honor validated hints.", DefaultValue = true)]
    public bool EarlyPoll { get; set; } = true;
}
