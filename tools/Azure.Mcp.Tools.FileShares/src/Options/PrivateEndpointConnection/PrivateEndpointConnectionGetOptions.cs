// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;

public class PrivateEndpointConnectionGetOptions : BaseFileSharesOptions
{
    public string? FileShareName { get; set; }
    public string? ConnectionName { get; set; }
}
