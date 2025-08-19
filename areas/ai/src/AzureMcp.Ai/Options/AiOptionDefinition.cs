// Copyright (c) Microsoft.
// Licensed under the MIT License.

using System.CommandLine;

namespace AzureMcp.Ai.Options;

public static class AiOptionDefinitions
{
    public static class Common
    {
        public static readonly Option<string> ResourceName =
            new(name: "--resource-name", description: "Azure OpenAI resource name (e.g., my-aoai).")
            { IsRequired = true };

        public static readonly Option<string> DeploymentName =
            new(name: "--deployment-name", description: "Azure OpenAI deployment name for completions.")
            { IsRequired = true };

        public static readonly Option<string> PromptText =
            new(name: "--prompt-text", description: "The prompt text to complete.")
            { IsRequired = true };

        public static readonly Option<int?> MaxTokens =
            new(name: "--max-tokens", description: "Maximum tokens to generate (default 256).");

        public static readonly Option<double?> Temperature =
            new(name: "--temperature", description: "Sampling temperature between 0 and 2 (default 0.7).");

        public static readonly Option<string?> ApiVersion =
            new(name: "--api-version", description: "Azure OpenAI API version (optional, e.g., 2023-05-15).");
    }
}