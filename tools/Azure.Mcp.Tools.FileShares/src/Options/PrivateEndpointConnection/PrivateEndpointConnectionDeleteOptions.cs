// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;

/// <summary>
/// Options for PrivateEndpointConnectionDeleteCommand.
/// </summary>
public class PrivateEndpointConnectionDeleteOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share.
    /// </summary>
    public string? FileShareName { get; set; }

    /// <summary>
    /// Gets or sets the name of the private endpoint connection to delete.
    /// </summary>
    public string? ConnectionName { get; set; }
}
