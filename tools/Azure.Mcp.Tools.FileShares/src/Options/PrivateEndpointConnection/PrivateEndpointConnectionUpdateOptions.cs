// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;

/// <summary>
/// Options for PrivateEndpointConnectionUpdateCommand.
/// </summary>
public class PrivateEndpointConnectionUpdateOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share.
    /// </summary>
    public string? FileShareName { get; set; }

    /// <summary>
    /// Gets or sets the name of the private endpoint connection.
    /// </summary>
    public string? ConnectionName { get; set; }

    /// <summary>
    /// Gets or sets the connection state (Approved, Rejected, or Removed).
    /// </summary>
    public string? ConnectionState { get; set; }

    /// <summary>
    /// Gets or sets the description for the connection state.
    /// </summary>
    public string? Description { get; set; }
}
