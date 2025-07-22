// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Options;
using AzureMcp.Areas.Startups.Services;
using AzureMcp.Commands;
using AzureMcp.Models.Option;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups.Commands;

public sealed class StartupsDeployCommand(ILogger<StartupsDeployCommand> logger) : GlobalCommand<StartupsDeployOptions>()
{
    private const string CommandTitle = "Deploy static web resources for startups";
    private readonly ILogger<StartupsDeployCommand> _logger = logger;

    public override string Name => "deploy";
    public override string Description =>
        $"""
        Deploy static web resources for startups. Requires subscription {OptionDefinitions.Common.SubscriptionName},
        resource group, storage account name, and source directory path. Configures static website hosting
        and uploads content from the specified directory.
        """;
    public override string Title => CommandTitle;

    [McpServerTool(Destructive = false, ReadOnly = true, Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }
        try
        {
            _logger.LogInformation("Starting deployment to storage account {StorageAccount}", options.StorageAccount);

            var startupsService = context.GetService<IStartupsService>();
            var result = await startupsService.DeployStaticWebAsync(options, CancellationToken.None);

            _logger.LogInformation("Successfully deployed to storage account {StorageAccount}", options.StorageAccount);
            context.Response.Results = ResponseResult.Create(result, DeployJsonContext.Default.StartupsDeployResources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying static website to {StorageAccount}", options.StorageAccount);
            HandleException(context, ex);
        }
    return context.Response;
    }
}

[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false, GenerationMode = System.Text.Json.Serialization.JsonSourceGenerationMode.Default)][System.Text.Json.Serialization.JsonSerializable(typeof(StartupsDeployResources))]
internal partial class DeployJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
