// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FoundryExtensions.Options;

public static class FoundryExtensionsOptionDefinitions
{
    public const string Endpoint = "endpoint";
    public const string DeploymentName = "deployment";
    public const string IndexName = "index";
    public const string PromptText = "prompt-text";
    public const string MaxTokens = "max-tokens";
    public const string Temperature = "temperature";
    public const string ResourceName = "resource-name";
    public const string InputText = "input-text";
    public const string User = "user";
    public const string EncodingFormat = "encoding-format";
    public const string Dimensions = "dimensions";
    public const string MessageArray = "message-array";
    public const string TopP = "top-p";
    public const string FrequencyPenalty = "frequency-penalty";
    public const string PresencePenalty = "presence-penalty";
    public const string Stop = "stop";
    public const string Stream = "stream";
    public const string Seed = "seed";

    public static readonly Option<string> EndpointOption = new(
        $"--{Endpoint}"
    )
    {
        Description = "The endpoint URL for the Microsoft Foundry project/service. The endpoint follows this pattern https://<foundry-resource-name>.services.ai.azure.com/api/projects/<project-name>.",
        Required = true
    };

    public static readonly Option<string> DeploymentNameOption = new(
        $"--{DeploymentName}"
    )
    {
        Description = "The name of the deployment.",
        Required = true
    };

    public static readonly Option<string> IndexNameOption = new(
        $"--{IndexName}"
    )
    {
        Description = "The name of the knowledge index.",
        Required = true
    };

    public static readonly Option<string> PromptTextOption = new(
        $"--{PromptText}"
    )
    {
        Description = "The prompt text to send to the completion model.",
        Required = true
    };

    public static readonly Option<string> ResourceNameOption = new(
        $"--{ResourceName}"
    )
    {
        Description = "The name of the Azure OpenAI resource.",
        Required = true
    };

    public static readonly Option<int> MaxTokensOption = new(
        $"--{MaxTokens}"
    )
    {
        Description = "The maximum number of tokens to generate in the completion."
    };

    public static readonly Option<double> TemperatureOption = new(
        $"--{Temperature}"
    )
    {
        Description = "Controls randomness in the output. Lower values make it more deterministic."
    };

    public static readonly Option<string> InputTextOption = new(
        $"--{InputText}"
    )
    {
        Description = "The input text to generate embeddings for.",
        Required = true
    };

    public static readonly Option<string> UserOption = new(
        $"--{User}"
    )
    {
        Description = "User identifier for tracking and abuse monitoring."
    };

    public static readonly Option<string> EncodingFormatOption = new(
        $"--{EncodingFormat}"
    )
    {
        Description = "The format to return embeddings in (float or base64).",
        DefaultValueFactory = _ => "float"
    };

    public static readonly Option<int> DimensionsOption = new(
        $"--{Dimensions}"
    )
    {
        Description = "The number of dimensions for the embedding output. Only supported in some models."
    };

    public static readonly Option<string> MessageArrayOption = new(
        $"--{MessageArray}"
    )
    {
        Description = "Array of messages in the conversation (JSON format). Each message should have 'role' and 'content' properties.",
        Required = true
    };

    public static readonly Option<double> TopPOption = new(
        $"--{TopP}"
    )
    {
        Description = "Controls diversity via nucleus sampling (0.0 to 1.0). Default is 1.0."
    };

    public static readonly Option<double> FrequencyPenaltyOption = new(
        $"--{FrequencyPenalty}"
    )
    {
        Description = "Penalizes new tokens based on their frequency (-2.0 to 2.0). Default is 0."
    };

    public static readonly Option<double> PresencePenaltyOption = new(
        $"--{PresencePenalty}"
    )
    {
        Description = "Penalizes new tokens based on presence (-2.0 to 2.0). Default is 0."
    };

    public static readonly Option<string> StopOption = new(
        $"--{Stop}"
    )
    {
        Description = "Up to 4 sequences where the API will stop generating further tokens."
    };

    public static readonly Option<bool> StreamOption = new(
        $"--{Stream}"
    )
    {
        Description = "Whether to stream back partial progress. Default is false."
    };

    public static readonly Option<int> SeedOption = new(
        $"--{Seed}"
    )
    {
        Description = "If specified, the system will make a best effort to sample deterministically."
    };
}
