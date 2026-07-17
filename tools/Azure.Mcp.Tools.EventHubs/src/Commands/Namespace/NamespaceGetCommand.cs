// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

[CommandMetadata(
    Id = "71ec6c5b-b6e4-4c64-b31b-2d61dfad3b5c",
    Name = "get",
    Title = "Get Event Hubs Namespaces",
    Description = """
        Get Event Hubs namespaces from Azure. This command supports three modes of operation:
        1. List all Event Hubs namespaces in a subscription 
        2. List all Event Hubs namespaces in a specific resource group 
        3. Get a single namespace by name 

        When retrieving a single namespace, detailed information including SKU, settings, and metadata 
        is returned. When listing namespaces, the same detailed information is returned for all 
        namespaces in the specified scope.

        The --resource-group parameter is optional for listing operations but required when getting a specific namespace.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class NamespaceGetCommand(ILogger<NamespaceGetCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<NamespaceGetOptions, NamespaceGetCommand.NamespaceGetCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<NamespaceGetCommand> _logger = logger;

    public override void ValidateOptions(NamespaceGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.Namespace) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            validationResult.Errors.Add("When specifying a namespace name, a resource group must also be provided.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, NamespaceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(options.Namespace))
            {
                // Get single namespace with detailed information
                var namespaceDetails = await _service.GetNamespaceAsync(
                    options.Namespace,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = namespaceDetails != null
                    ? ResponseResult.Create(new(namespaceDetails, null), EventHubsJsonContext.Default.NamespaceGetCommandResult)
                    : null;
            }
            else
            {
                var namespaces = await _service.GetNamespacesAsync(
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, namespaces ?? []), EventHubsJsonContext.Default.NamespaceGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Event Hubs namespaces");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record NamespaceGetCommandResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Models.Namespace? Namespace,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<Models.Namespace>? Namespaces);
}
