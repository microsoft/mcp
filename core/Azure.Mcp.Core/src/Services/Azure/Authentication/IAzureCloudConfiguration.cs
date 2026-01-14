// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Provides configuration for Azure cloud environments.
/// </summary>
public interface IAzureCloudConfiguration
{
    /// <summary>
    /// Gets the authority host URI for the Azure cloud environment.
    /// </summary>
    Uri AuthorityHost { get; }
}
