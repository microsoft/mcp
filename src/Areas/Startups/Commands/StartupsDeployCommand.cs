// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Messaging.ServiceBus.Administration;
using AzureMcp.Areas.Startups.Options;
using AzureMcp.Areas.Startups.Services;
using AzureMcp.Commands;
using Microsoft.Extensions.Logging;

namespace AzureMcp.Areas.Startups.Commands;

public sealed class StartupsDeployCommand(
    IStartupsServices startupsServices,
    ILogger<StartupsDeployCommand> logger
) : GlobalCommand<StartupsDeployOptions>()
{
    private const string CommandTitle = "Deploy static web resources for startups";
    private readonly IStartupsServices _startupsServices = startupsServices;
    private readonly ILogger<StartupsDeployCommand> _logger = logger;

    public override string Name => "deploy";
    public override string Description => "Deploy static web resources for startups";
    public override string Title => CommandTitle;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        var result = await _startupsServices.DeployStaticWebAsync(options, CancellationToken.None);
        context.Response.Results = ResponseResult.Create(
            result, DeployJsonContext.Default.StartupsDeployResources);
        return context.Response;
    }
}

[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false, GenerationMode = System.Text.Json.Serialization.JsonSourceGenerationMode.Default)]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsDeployResources))]
internal partial class DeployJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
