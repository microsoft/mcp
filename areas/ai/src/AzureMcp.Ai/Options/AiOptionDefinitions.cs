// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AzureMcp.Ai.Options;

public static class AiOptionDefinitions
{
    public const string ResourceNameName = "resource-name";
    public const string DeploymentNameName = "deployment-name";
    public const string PromptTextName = "prompt-text";
    public const string MaxTokensName = "max-tokens";
    public const string TemperatureName = "temperature";

    public static readonly Option<string> ResourceName = new(
        $"--{ResourceNameName}",
        "The name of the Azure OpenAI resource."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> DeploymentName = new(
        $"--{DeploymentNameName}",
        "The name of the deployment within the Azure OpenAI resource."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> PromptText = new(
        $"--{PromptTextName}",
        "The prompt text to send to the Azure OpenAI completion model."
    )
    {
        IsRequired = true
    };

    public static readonly Option<int> MaxTokens = new(
        $"--{MaxTokensName}",
        "The maximum number of tokens to generate in the completion."
    )
    {
        IsRequired = false
    };

    public static readonly Option<double> Temperature = new(
        $"--{TemperatureName}",
        "Controls randomness in the output. Values between 0.0 and 1.0."
    )
    {
        IsRequired = false
    };
}
