// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Foundry.Options;

public static class FoundryOptionDefinitions
{
    public const string Endpoint = "endpoint";
    public const string SearchForFreePlayground = "search-for-free-playground";
    public const string PublisherName = "publisher";
    public const string LicenseName = "license";
    public const string DeploymentName = "deployment";
    public const string ModelName = "model-name";
    public const string ModelFormat = "model-format";
    public const string AzureAiServicesName = "azure-ai-services";
    public const string ModelVersion = "model-version";
    public const string ModelSource = "model-source";
    public const string SkuName = "sku";
    public const string SkuCapacity = "sku-capacity";
    public const string ScaleType = "scale-type";
    public const string ScaleCapacity = "scale-capacity";
    public const string AgentId = "agent-id";
    public const string Query = "query";
    public const string Evaluators = "evaluators";
    public const string EvaluatorName = "evaluator";
    public const string Response = "response";
    public const string ToolCalls = "tool-calls";
    public const string ToolDefinitions = "tool-definitions";
    public const string AzureOpenAIEndpoint = "azure-openai-endpoint";
    public const string AzureOpenAIDeployment = "azure-openai-deployment";
    public const string IndexName = "index";

    public static readonly Option<string> EndpointOption = new(
        $"--{Endpoint}"
    )
    {
        Description = "The endpoint URL for the Azure AI service.",
        Required = true
    };

    public static readonly Option<string> DeploymentNameOption = new(
        $"--{DeploymentName}"
    )
    {
        Description = "The name of the deployment.",
        Required = true
    };

    public static readonly Option<string> ModelNameOption = new(
        $"--{ModelName}"
    )
    {
        Description = "The name of the model to deploy.",
        Required = true
    };

    public static readonly Option<string> ModelFormatOption = new(
        $"--{ModelFormat}"
    )
    {
        Description = "The format of the model (e.g., 'OpenAI', 'Meta', 'Microsoft').",
        Required = true
    };

    public static readonly Option<string> AzureAiServicesNameOption = new(
        $"--{AzureAiServicesName}"
    )
    {
        Description = "The name of the Azure AI services account to deploy to.",
        Required = true
    };

    public static readonly Option<bool> SearchForFreePlaygroundOption = new(
        $"--{SearchForFreePlayground}"
    )
    {
        Description = "If true, filters models to include only those that can be used for free by users for prototyping."
    };

    public static readonly Option<string> PublisherNameOption = new(
        $"--{PublisherName}"
    )
    {
        Description = "A filter to specify the publisher of the models to retrieve."
    };

    public static readonly Option<string> LicenseNameOption = new(
        $"--{LicenseName}"
    )
    {
        Description = "A filter to specify the license type of the models to retrieve."
    };

    public static readonly Option<string> ModelVersionOption = new(
        $"--{ModelVersion}"
    )
    {
        Description = "The version of the model to deploy."
    };

    public static readonly Option<string> ModelSourceOption = new(
        $"--{ModelSource}"
    )
    {
        Description = "The source of the model."
    };

    public static readonly Option<string> SkuNameOption = new(
        $"--{SkuName}"
    )
    {
        Description = "The SKU name for the deployment."
    };

    public static readonly Option<int> SkuCapacityOption = new(
        $"--{SkuCapacity}"
    )
    {
        Description = "The SKU capacity for the deployment."
    };

    public static readonly Option<string> ScaleTypeOption = new(
        $"--{ScaleType}"
    )
    {
        Description = "The scale type for the deployment."
    };

    public static readonly Option<int> ScaleCapacityOption = new(
        $"--{ScaleCapacity}"
    )
    {
        Description = "The scale capacity for the deployment."
    };

    public static readonly Option<string> AgentIdOption = new(
        $"--{AgentId}"
    )
    {
        Description = "The ID of the agent to interact with.",
        Required = true
    };

    public static readonly Option<string> QueryOption = new(
        $"--{Query}"
    )
    {
        Description = "The query sent to the agent.",
        Required = true
    };

    public static readonly Option<string> EvaluatorsOption = new(
        $"--{Evaluators}"
    )
    {
        Description = "The list of evaluators to use for evaluation, separated by commas. If not specified, all evaluators will be used."
    };

    public static readonly Option<string> EvaluatorNameOption = new(
        $"--{EvaluatorName}"
    )
    {
        Description = "The name of the evaluator to use (intent_resolution, tool_call_accuracy, task_adherence).",
        Required = true
    };

    public static readonly Option<string> ResponseOption = new(
        $"--{Response}"
    )
    {
        Description = "The response from the agent.",
    };

    public static readonly Option<string> ToolCallsOption = new(
        $"--{ToolCalls}"
    )
    {
        Description = "The tool calls made by the agent."
    };

    public static readonly Option<string> ToolDefinitionsOption = new(
        $"--{ToolDefinitions}"
    )
    {
        Description = "Optional tool definitions made by the agent in JSON format."
    };

    public static readonly Option<string> AzureOpenAIEndpointOption = new(
        $"--{AzureOpenAIEndpoint}"
    )
    {
        Description = "The endpoint URL for the Azure OpenAI service to be used in evaluation.",
        Required = true
    };

    public static readonly Option<string> AzureOpenAIDeploymentOption = new(
        $"--{AzureOpenAIDeployment}"
    )
    {
        Description = "The deployment name for the Azure OpenAI model to be used in evaluation.",
        Required = true
    };

    public static readonly Option<string> IndexNameOption = new(
        $"--{IndexName}"
    )
    {
        Description = "The name of the knowledge index.",
        Required = true
    };
}
