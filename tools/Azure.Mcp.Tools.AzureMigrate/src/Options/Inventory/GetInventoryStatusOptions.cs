// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Inventory;

/// <summary>
/// Options for the inventory status command.
/// </summary>
public sealed class GetInventoryStatusOptions
{
    /// <summary>
    /// Gets or sets the inventory session ID.
    /// </summary>
    [Option(Description = "Required. ID of the inventory session (sessionId) returned when the inventory was created or updated.")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets the operation ID.
    /// </summary>
    [Option(Description = "Required. ID of the inventory operation (operationId, a GUID) returned when the inventory was created or updated.")]
    public string? OperationId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to return the current status straight away instead of waiting for completion.
    /// </summary>
    [Option(Description = "Optional. Set true to return the current status straight away instead of waiting up to about 25 seconds for completion. Used by the upload panel.")]
    public bool? NoWait { get; set; }
}
