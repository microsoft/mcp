// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options;

public class PrivateEndpointConnectionUpdateOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the storage account name.
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// Gets or sets the private endpoint connection name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the connection status (Approved, Rejected).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the reason for approval/rejection.
    /// </summary>
    public string? Description { get; set; }
}
