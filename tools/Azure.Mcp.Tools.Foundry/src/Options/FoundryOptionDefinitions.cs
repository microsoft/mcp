// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Foundry.Options;

public static class FoundryOptionDefinitions
{
    public const string Endpoint = "endpoint";
    public const string SearchForFreePlayground = "search-for-free-playground";
    public const string PublisherName = "publisher";
    public const string LicenseName = "license";
    public const string OptionalModelName = "model-name";
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

    public static readonly Option<string> EndpointOption = new(
        $"--{Endpoint}",
        "The endpoint URL for the Azure AI service."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> OptionalModelNameOption = new(
        $"--{ModelName}",
        "The name of the model to search for."
    );


    public static readonly Option<string> DeploymentNameOption = new(
        $"--{DeploymentName}",
        "The name of the deployment."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> ModelNameOption = new(
        $"--{ModelName}",
        "The name of the model to deploy."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> ModelFormatOption = new(
        $"--{ModelFormat}",
        "The format of the model (e.g., 'OpenAI', 'Meta', 'Microsoft')."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> AzureAiServicesNameOption = new(
        $"--{AzureAiServicesName}",
        "The name of the Azure AI services account to deploy to."
    )
    {
        IsRequired = true
    };

    public static readonly Option<bool> SearchForFreePlaygroundOption = new(
        $"--{SearchForFreePlayground}",
        "If true, filters models to include only those that can be used for free by users for prototyping."
    );

    public static readonly Option<string> PublisherNameOption = new(
        $"--{PublisherName}",
        "A filter to specify the publisher of the models to retrieve."
    );

    public static readonly Option<string> LicenseNameOption = new(
        $"--{LicenseName}",
        "A filter to specify the license type of the models to retrieve."
    );

    public static readonly Option<string> ModelVersionOption = new(
        $"--{ModelVersion}",
        "The version of the model to deploy."
    );

    public static readonly Option<string> ModelSourceOption = new(
        $"--{ModelSource}",
        "The source of the model."
    );

    public static readonly Option<string> SkuNameOption = new(
        $"--{SkuName}",
        "The SKU name for the deployment."
    );

    public static readonly Option<int> SkuCapacityOption = new(
        $"--{SkuCapacity}",
        "The SKU capacity for the deployment."
    );

    public static readonly Option<string> ScaleTypeOption = new(
        $"--{ScaleType}",
        "The scale type for the deployment."
    );

    public static readonly Option<int> ScaleCapacityOption = new(
        $"--{ScaleCapacity}",
        "The scale capacity for the deployment."
    );
}
