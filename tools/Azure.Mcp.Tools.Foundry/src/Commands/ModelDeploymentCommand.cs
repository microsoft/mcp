// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class ModelDeploymentCommand : SubscriptionCommand<ModelDeploymentOptions>
{
    private const string CommandTitle = "Deploy Model to Azure AI Services";

    public override string Name => "deploy";

    public override string Description =>
    "Deploys (create) a model instance (e.g. GPT4o / gpt-4o, OpenAI, OSS, proprietary) in Azure AI Foundry as a named inference deployment, which's bound to a specified Azure AI Services resource, resource group, and subscription. Do not use this tool for for Azure OpenAI Services to deploy OpenAI models.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FoundryOptionDefinitions.DeploymentNameOption);
        command.Options.Add(FoundryOptionDefinitions.ModelNameOption);
        command.Options.Add(FoundryOptionDefinitions.ModelFormatOption);
        command.Options.Add(FoundryOptionDefinitions.AzureAiServicesNameOption);
        command.Options.Add(FoundryOptionDefinitions.ModelVersionOption);
        command.Options.Add(FoundryOptionDefinitions.ModelSourceOption);
        command.Options.Add(FoundryOptionDefinitions.SkuNameOption);
        command.Options.Add(FoundryOptionDefinitions.SkuCapacityOption);
        command.Options.Add(FoundryOptionDefinitions.ScaleTypeOption);
        command.Options.Add(FoundryOptionDefinitions.ScaleCapacityOption);
    }

    protected override ModelDeploymentOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.DeploymentName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.DeploymentNameOption.Name);
        options.ModelName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ModelNameOption.Name);
        options.ModelFormat = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ModelFormatOption.Name);
        options.AzureAiServicesName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.AzureAiServicesNameOption.Name);
        options.ModelVersion = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ModelVersionOption.Name);
        options.ModelSource = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ModelSourceOption.Name);
        options.SkuName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.SkuNameOption.Name);
        options.SkuCapacity = parseResult.GetValueOrDefault<int>(FoundryOptionDefinitions.SkuCapacityOption.Name);
        options.ScaleType = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ScaleTypeOption.Name);
        options.ScaleCapacity = parseResult.GetValueOrDefault<int>(FoundryOptionDefinitions.ScaleCapacityOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {

            var service = context.GetService<IFoundryService>();
            var deploymentResource = await service.DeployModel(
                options.DeploymentName!,
                options.ModelName!,
                options.ModelFormat!,
                options.AzureAiServicesName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.ModelVersion,
                options.ModelSource,
                options.SkuName,
                options.SkuCapacity,
                options.ScaleType,
                options.ScaleCapacity,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(deploymentResource), FoundryJsonContext.Default.ModelDeploymentCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ModelDeploymentCommandResult(ModelDeploymentResult DeploymentData);
}
