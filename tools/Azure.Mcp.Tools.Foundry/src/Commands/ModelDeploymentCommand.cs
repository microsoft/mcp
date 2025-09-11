// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class ModelDeploymentCommand : SubscriptionCommand<ModelDeploymentOptions>
{
    private const string CommandTitle = "Deploy Model to Azure AI Services";
    private readonly Option<string> _deploymentNameOption = FoundryOptionDefinitions.DeploymentNameOption;
    private readonly Option<string> _modelNameOption = FoundryOptionDefinitions.ModelNameOption;
    private readonly Option<string> _modelFormatOption = FoundryOptionDefinitions.ModelFormatOption;
    private readonly Option<string> _azureAiServicesNameOption = FoundryOptionDefinitions.AzureAiServicesNameOption;
    private readonly Option<string> _modelVersionOption = FoundryOptionDefinitions.ModelVersionOption;
    private readonly Option<string> _modelSourceOption = FoundryOptionDefinitions.ModelSourceOption;
    private readonly Option<string> _skuNameOption = FoundryOptionDefinitions.SkuNameOption;
    private readonly Option<int> _skuCapacityOption = FoundryOptionDefinitions.SkuCapacityOption;
    private readonly Option<string> _scaleTypeOption = FoundryOptionDefinitions.ScaleTypeOption;
    private readonly Option<int> _scaleCapacityOption = FoundryOptionDefinitions.ScaleCapacityOption;

    public override string Name => "deploy";

    public override string Description =>
        """
        Deploy a model to Azure AI Foundry.

        This function is used to deploy a model on Azure AI Services, allowing users to integrate the model into their applications and utilize its capabilities. This command should not be used for Azure OpenAI Services to deploy OpenAI models.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_deploymentNameOption);
        command.Options.Add(_modelNameOption);
        command.Options.Add(_modelFormatOption);
        command.Options.Add(_azureAiServicesNameOption);
        RequireResourceGroup();
        command.Options.Add(_modelVersionOption);
        command.Options.Add(_modelSourceOption);
        command.Options.Add(_skuNameOption);
        command.Options.Add(_skuCapacityOption);
        command.Options.Add(_scaleTypeOption);
        command.Options.Add(_scaleCapacityOption);
    }

    protected override ModelDeploymentOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DeploymentName = parseResult.GetValue(_deploymentNameOption);
        options.ModelName = parseResult.GetValue(_modelNameOption);
        options.ModelFormat = parseResult.GetValue(_modelFormatOption);
        options.AzureAiServicesName = parseResult.GetValue(_azureAiServicesNameOption);
        options.ModelVersion = parseResult.GetValue(_modelVersionOption);
        options.ModelSource = parseResult.GetValue(_modelSourceOption);
        options.SkuName = parseResult.GetValue(_skuNameOption);
        options.SkuCapacity = parseResult.GetValue(_skuCapacityOption);
        options.ScaleType = parseResult.GetValue(_scaleTypeOption);
        options.ScaleCapacity = parseResult.GetValue(_scaleCapacityOption);

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

            context.Response.Results =
                ResponseResult.Create(
                    new ModelDeploymentCommandResult(deploymentResource),
                    FoundryJsonContext.Default.ModelDeploymentCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ModelDeploymentCommandResult(ModelDeploymentResult DeploymentData);
}
