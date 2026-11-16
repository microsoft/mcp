// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.Inventory;

/// <summary>
/// Options for the inventory create-or-update command.
/// </summary>
public sealed class CreateOrUpdateInventoryOptions
{
    /// <summary>
    /// Gets or sets the session to update. When omitted, a new session is created, or the caller's active one is reused.
    /// </summary>
    [Option(Description = "Optional. ID of the inventory session to update, as returned by an earlier call (1-20 characters: letters, digits, '-' or '_'). Omit to create a new inventory; if the user already has an active session it is reused.")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets or sets optional instructions passed to the skill with the invocation.
    /// </summary>
    [Option(Description = "Optional instructions for the inventory build, for example 'only include production servers'. Omit when the user gave none.")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets whether the update is driven by <see cref="Message"/> alone, without uploading a file.
    /// </summary>
    [Option(Description = "Set true to update an existing inventory from 'message' alone, without uploading a file: a correction such as " +
        "'set server web-01's OS to Windows Server 2025', guidance such as 'memory is in GB', or a question about the inventory. " +
        "Requires 'session-id' and 'message'. Leave false when the user provides a new or edited file.")]
    public bool TextOnly { get; set; }
}
