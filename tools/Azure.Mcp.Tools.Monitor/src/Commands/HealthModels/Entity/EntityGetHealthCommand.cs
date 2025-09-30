// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;

public sealed class EntityGetHealthCommand(ILogger<EntityGetHealthCommand> logger) : BaseMonitorHealthModelsCommand<BaseMonitorHealthModelsOptions>
{
    private const string CommandTitle = "Get the health of an entity in a health model";
    private const string CommandName = "gethealth";
    public override string Name => CommandName;

    public override string Description =>
         $"""
        Retrieves the health status and history of a specific entity from an Azure Monitor Health Model. Shows detailed health information including availability state, health events, and timestamps for the specified entity.
        Required arguments: entity name and health model name.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    private readonly ILogger<EntityGetHealthCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IMonitorHealthModelService>();
            var result = await service.GetEntityHealth(
                options.Entity!,
                options.HealthModelName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.AuthMethod,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(result, MonitorJsonContext.Default.JsonNode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "An exception occurred getting health for entity: {Entity} in healthModel: {HealthModelName}, resourceGroup: {ResourceGroup}, subscription: {Subscription}, authMethod: {AuthMethod}"
                + ", tenant: {Tenant}.",
                options.Entity,
                options.HealthModelName,
                options.ResourceGroup,
                options.Subscription,
                options.AuthMethod,
                options.Tenant);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => $"Entity or health model not found. Please check the entity ID, health model name, and resource group.",
        ArgumentException argEx => $"Invalid argument: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => HttpStatusCode.NotFound,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };
}
