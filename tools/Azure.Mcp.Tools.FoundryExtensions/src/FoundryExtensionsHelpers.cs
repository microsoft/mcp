// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.FoundryExtensions;

internal static class FoundryExtensionsHelpers
{
    internal static void ValidateFoundryEndpoint(string endpoint, ValidationResult validationResult)
    {
        ArmEnvironment[] clouds = [ArmEnvironment.AzurePublicCloud, ArmEnvironment.AzureChina, ArmEnvironment.AzureGovernment, ArmEnvironment.AzureGermany];
        string? lastError = null;

        foreach (var cloud in clouds)
        {
            try
            {
                EndpointValidator.ValidateAzureServiceEndpoint(endpoint, "foundry", cloud);
                return;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
        }

        validationResult.Errors.Add(lastError ?? $"Invalid Foundry project endpoint: {endpoint}");
    }
}
