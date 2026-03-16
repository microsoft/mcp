// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;

public class PrivateEndpointConnectionUpdateOptions : BaseFileSharesOptions
{
    public string? FileShareName { get; set; }
    public string? ConnectionName { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
}
