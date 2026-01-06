// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;

/// <summary>
/// Options for PrivateEndpointConnectionListCommand.
/// </summary>
public class PrivateEndpointConnectionListOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share.
    /// </summary>
    public string? FileShareName { get; set; }
}
