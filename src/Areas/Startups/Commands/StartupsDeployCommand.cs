// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Core.Pipeline;
using Azure.ResourceManager.Resources.Models;
using AzureMcp.Areas.Startups.Options;
using AzureMcp.Areas.Startups.Services;
using AzureMcp.Commands.Subscription;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups.Commands;

public sealed class StartupsDeployCommand(ILogger<StartupsDeployCommand> logger) : SubscriptionCommand<StartupsDeployOptions>()
{
    private const string CommandTitle = "Deploy Static Website for Startups";
    private readonly ILogger<StartupsDeployCommand> _logger = logger;

    private readonly Option<string> _subscription = StartupsOptionDefinitions.Subscription;

    public override string Name => "deploy";
    public override string Description =>
        """
        Deploy static web resources for startups. Requires subscription {OptionDefinitions.Common.SubscriptionName},
        resource group, storage account name, and source directory path. Configures static website hosting
        and uploads content from the specified directory.
        """;
    public override string Title => CommandTitle;

    // protected override void RegisterOptions(Command command)
    // {
    //     base.RegisterOptions(command);
    //     command.AddOption(_subscription);
    // }

    // protected override StartupsDeployOptions BindOptions(ParseResult parseResult)
    // {
    //     var options = base.BindOptions(parseResult);
    //     options.SubscriptionId = parseResult.GetValueForOption(_subscription)
    //         ?? throw new ArgumentNullException(nameof(options.SubscriptionId), "Subscription cannot be null.");
    //     return options;
    // }

    [McpServerTool(Destructive = false, ReadOnly = true, Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var subscription = parseResult.GetValueForOption<string>(StartupsOptionDefinitions.Subscription);
        var storageAccount = parseResult.GetValueForOption<string>(StartupsOptionDefinitions.StorageAccount);
        var resourceGroup = parseResult.GetValueForOption<string>(StartupsOptionDefinitions.ResourceGroup);
        var sourcePath = parseResult.GetValueForOption<string>(StartupsOptionDefinitions.SourcePath);

        if (subscription is null)
            throw new ArgumentNullException(nameof(subscription), "Subscription cannot be null.");
        if (storageAccount is null)
            throw new ArgumentNullException(nameof(storageAccount), "Storage account cannot be null.");
        if (resourceGroup is null)
            throw new ArgumentNullException(nameof(resourceGroup), "Resource group cannot be null.");
        if (sourcePath is null)
            throw new ArgumentNullException(nameof(sourcePath), "Source path cannot be null.");

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }
            _logger.LogInformation("Starting deployment to storage account {StorageAccount}", storageAccount);

            var startupsService = context.GetService<IStartupsService>();

            var result = await startupsService.DeployStaticWebAsync(subscription, storageAccount, resourceGroup, sourcePath);

            _logger.LogInformation("Successfully deployed to storage account {StorageAccount}", storageAccount);
            context.Response.Results = ResponseResult.Create(result, DeployJsonContext.Default.StartupsDeployResources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying static website to {StorageAccount}", storageAccount);
            HandleException(context, ex);
        }
        return context.Response;
    }
}

[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false, GenerationMode = System.Text.Json.Serialization.JsonSourceGenerationMode.Default)][System.Text.Json.Serialization.JsonSerializable(typeof(StartupsDeployResources))]
internal partial class DeployJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
