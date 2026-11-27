// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

internal static class ResiliencyAgentCloudSupport
{
    internal static void EnsureSupported(IAzureCloudConfiguration cloudConfiguration)
    {
        if (cloudConfiguration.CloudType != AzureCloudConfiguration.AzureCloud.AzurePublicCloud)
        {
            throw new InvalidOperationException(
                "The Azure Resiliency Agent is currently supported only in Azure public cloud. " +
                $"The configured cloud '{cloudConfiguration.CloudType}' is not supported.");
        }
    }
}
