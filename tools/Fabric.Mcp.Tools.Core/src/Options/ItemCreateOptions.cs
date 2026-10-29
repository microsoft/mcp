// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public class ItemCreateOptions
{
    [Option(Description = "The nonempty UUID of the Microsoft Fabric workspace. Workspace names are not resolved. Takes precedence over workspace unless omitted, empty, or whitespace.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "Backward-compatible alias for the workspace UUID, used when workspace-id is omitted or blank. Workspace names are not supported.")]
    public string? Workspace { get; set; }

    [Option(Description = "The display name for the item, following the naming rules for its item type.")]
    public required string DisplayName { get; set; }

    [Option(Description = "The type of the Fabric item (e.g., Lakehouse, Notebook, etc.).")]
    public required string ItemType { get; set; }

    [Option(Description = "The description for the item, at most 256 characters.")]
    public string? Description { get; set; }

    [Option(Description = "Optional nonempty UUID of an existing Fabric folder in the workspace. Passed directly without looking up or creating folders. Omit to use the workspace root.")]
    public string? FolderId { get; set; }

    [Option(Description = "Item-specific creation properties as a JSON object string, up to 1 MiB (1048576 UTF-8 bytes) and 32 nesting levels. Mutually exclusive with every inline or saved-file definition option. Fabric validates supported properties for the item type. Not a path or a complete create-request body.")]
    public string? CreationPayload { get; set; }

    [Option(Description = "The API's top-level item-specific options as a JSON object string, up to 1 MiB (1048576 UTF-8 bytes) and 32 nesting levels. Requires an inline or saved-file definition source. Consult the item's Update Definition API for supported options. These are not LRO wait settings; use sync, max-wait-seconds, and early-poll for waiting.")]
    public string? Options { get; set; }

    [Option(Description = "Optional label settings as a JSON object string, up to 4096 UTF-8 bytes: {\"labelId\":\"nonempty UUID\",\"sensitivityLabelApplyStrategy\":\"ApplyOrFail\"}. Both fields are optional; omitted fields stay omitted. A supplied strategy must be ApplyOrFail or Ignore. Fabric defaults an omitted strategy to ApplyOrFail. Ignore explicitly permits creation without the label if it cannot be applied; the tool never downgrades policy or retries without it.")]
    public string? SensitivityLabelSettings { get; set; }

    [OptionContainer<DefinitionInputOptions>(Prefix = "")]
    public DefinitionInputOptions? DefinitionInput { get; set; }

    [Option(Description = "Wait asynchronously for accepted creation to complete. Defaults to false, returning an operation receipt without polling.", DefaultValue = false)]
    public bool Sync { get; set; }

    [Option(Description = "Finite positive local wait budget in seconds after an accepted response when sync is true. Defaults to 120; the server rejects values above 600 (10 minutes), without clamping. Fractional seconds are allowed. Expiry returns a resumable receipt and does not cancel Fabric's operation.", DefaultValue = FabricOperationWaitOptions.DefaultMaxWaitSeconds)]
    public double MaxWaitSeconds { get; set; } = FabricOperationWaitOptions.DefaultMaxWaitSeconds;

    [Option(Description = "Allow one early status probe after at most 3 seconds, even if the initial 202 Retry-After is longer. Defaults to true; false honors the initial hint. Later polls and 429/503 retries honor validated hints.", DefaultValue = true)]
    public bool EarlyPoll { get; set; } = true;
}
