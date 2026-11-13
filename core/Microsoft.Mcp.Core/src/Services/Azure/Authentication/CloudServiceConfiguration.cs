// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Configures a cloud service endpoint and its OAuth audience.
/// </summary>
public sealed record CloudServiceConfiguration(Uri Endpoint, string Audience)
{
    /// <summary>
    /// Gets the default OAuth scope for the configured audience.
    /// </summary>
    public string DefaultScope => $"{Audience.TrimEnd('/')}/.default";
}
