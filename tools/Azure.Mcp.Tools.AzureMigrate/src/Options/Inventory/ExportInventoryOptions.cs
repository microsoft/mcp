// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Inventory;

/// <summary>
/// Options for the inventory export command.
/// </summary>
public sealed class ExportInventoryOptions
{
    /// <summary>
    /// Gets or sets the inventory session ID.
    /// </summary>
    [Option(Description = "Required. ID of the inventory session (sessionId) returned when the inventory was created or updated.")]
    public string? SessionId { get; set; }
}
