// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FoundryExtensions.Options;

internal static class FoundryExtensionsOptionDescriptions
{
    internal const string Endpoint = "The endpoint URL for the Microsoft Foundry project/service. The endpoint follows this pattern https://<foundry-resource-name>.services.ai.azure.com/api/projects/<project-name>.";
    internal const string Deployment = "The name of the deployment.";
    internal const string ResourceName = "The name of the Azure OpenAI resource.";
    internal const string MaxTokens = "The maximum number of tokens to generate in the completion.";
    internal const string Temperature = "Controls randomness in the output. Lower values make it more deterministic.";
    internal const string User = "User identifier for tracking and abuse monitoring.";
}
