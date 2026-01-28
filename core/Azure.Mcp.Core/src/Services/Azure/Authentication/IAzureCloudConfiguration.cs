// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager;

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

    /// <summary>
    /// Gets the ARM environment for the Azure cloud.
    /// This determines the management endpoint used for Azure Resource Manager operations.
    /// </summary>
    ArmEnvironment ArmEnvironment { get; }
}
