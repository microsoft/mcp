// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResourceHealth.Options.AvailabilityStatus;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ResourceHealth.Commands.AvailabilityStatus;

/// <summary>
/// Gets or lists availability status information for Azure resources.
/// </summary>
[CommandMetadata(
    Id = "3b388cc7-4b16-4919-9e90-f592247d9891",
    Name = "get",
    Title = "Get/List Resource Availability Status",
    Description = "Get the Azure Resource Health availability status for a specific resource or all resources in a subscription or resource group. Use this tool when asked about the availability status, health status, or Resource Health of an Azure resource (e.g. virtual machine, storage account). Reports whether a resource is Available, Unavailable, Degraded, or Unknown, including the reason and details. This is the correct tool for questions like 'What is the availability status of VM X?' or 'Is resource Y healthy?'.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AvailabilityStatusGetCommand(ILogger<AvailabilityStatusGetCommand> logger, IResourceHealthService resourceHealthService)
    : BaseResourceHealthCommand<AvailabilityStatusGetOptions>()
{
    private readonly ILogger<AvailabilityStatusGetCommand> _logger = logger;
    private readonly IResourceHealthService _resourceHealthService = resourceHealthService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ResourceHealthOptionDefinitions.ResourceId.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override AvailabilityStatusGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceId = parseResult.GetValueOrDefault<string>(ResourceHealthOptionDefinitions.ResourceId.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            List<Models.AvailabilityStatus> statuses;

            // If resourceId is provided, get single resource status
            if (!string.IsNullOrEmpty(options.ResourceId))
            {
                var status = await _resourceHealthService.GetAvailabilityStatusAsync(
                    options.ResourceId,
                    options.RetryPolicy,
                    cancellationToken);

                statuses = [status];
            }
            // Otherwise, list all resources
            else
            {
                statuses = await _resourceHealthService.ListAvailabilityStatusesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken) ?? [];
            }

            context.Response.Results = ResponseResult.Create(
                new(statuses),
                ResourceHealthJsonContext.Default.AvailabilityStatusGetCommandResult);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(options.ResourceId))
            {
                _logger.LogError(ex, "Failed to get availability status for resource {ResourceId}", options.ResourceId);
            }
            else
            {
                _logger.LogError(ex, "Failed to list availability statuses for subscription {Subscription}{ResourceGroupInfo}",
                    options.Subscription,
                    options.ResourceGroup != null ? $" and resource group {options.ResourceGroup}" : "");
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ResourceHealthUnsupportedResourceException unsupportedEx =>
            $"Resource Health availability status is not supported for resource type '{unsupportedEx.ResourceType}'. Use a supported Azure resource type, or list availability statuses at the subscription or resource group scope",
        HttpRequestException httpEx => httpEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ResourceHealthUnsupportedResourceException unsupportedEx => unsupportedEx.StatusCode,
        HttpRequestException httpEx when httpEx.StatusCode.HasValue => httpEx.StatusCode.Value,
        _ => base.GetStatusCode(ex)
    };

    internal record AvailabilityStatusGetCommandResult(List<Models.AvailabilityStatus> Statuses);
}
