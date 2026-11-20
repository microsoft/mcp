// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Configures trusted Azure Data Explorer endpoints and authentication.
/// </summary>
public sealed record KustoCloudConfiguration(string EndpointSuffix, string Audience)
{
    /// <summary>
    /// Gets the default OAuth scope for the configured audience.
    /// </summary>
    public string DefaultScope => $"{Audience.TrimEnd('/')}/.default";
}
