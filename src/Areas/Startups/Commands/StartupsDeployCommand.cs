// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Messaging.ServiceBus.Administration;
using AzureMcp.Areas.Startups.Options;
using AzureMcp.Areas.Startups.Services;
using AzureMcp.Commands;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups.Commands;

public sealed class StartupsDeployCommand(ILogger<StartupsDeployCommand> logger) : GlobalCommand<StartupsDeployOptions>()
{
    private const string CommandTitle = "Deploy static web resources for startups";
    private readonly ILogger<StartupsDeployCommand> _logger = logger;

    public override string Name => "deploy";
    public override string Description => "Deploy static web resources for startups";
    public override string Title => CommandTitle;

    [McpServerTool(Destructive = false, ReadOnly = true, Title = CommandTitle)]
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        
        try
        {
            var startupsService = context.GetService<IStartupsService>();
            var subscriptions = await startupsService.DeployStaticWebAsync(options, CancellationToken.None);

            context.Response.Results = ResponseResult.Create<StartupsDeployResources>(
                subscriptions, DeployJsonContext.Default.StartupsDeployResources);
            
            _logger.LogInformation("Successfully deployed static website to {StorageAccount}", options.StorageAccount);
            
            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deploy static website to {StorageAccount}", options.StorageAccount);
            throw;
        }
    }
}

[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false, GenerationMode = System.Text.Json.Serialization.JsonSourceGenerationMode.Default)][System.Text.Json.Serialization.JsonSerializable(typeof(StartupsDeployResources))]
internal partial class DeployJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
