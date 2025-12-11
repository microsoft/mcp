// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options;

public class PrivateEndpointConnectionGetOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the storage account name.
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// Gets or sets the private endpoint connection name.
    /// </summary>
    public string? Name { get; set; }
}
