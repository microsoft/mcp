// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

public sealed class NamespaceGetCommand(ILogger<NamespaceGetCommand> logger)
    : BaseEventHubsCommand<NamespaceGetOptions>
{
    private const string CommandTitle = "Get Event Hubs Namespaces";

    private readonly ILogger<NamespaceGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Get Event Hubs namespaces from Azure. This command supports three modes of operation:
        1. List all Event Hubs namespaces in a subscription (when no --resource-group is provided)
        2. List all Event Hubs namespaces in a specific resource group (when only --resource-group is provided)
        3. Get a single namespace by name (using --namespace with --resource-group)
        
        When retrieving a single namespace, detailed information including SKU, settings, and metadata 
        is returned. When listing namespaces, the same detailed information is returned for all 
        namespaces in the specified scope.
        
        The --resource-group parameter is optional for listing operations but required when getting a specific namespace.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = false,   // Safe read-only operation
        Idempotent = true,     // Same parameters produce same results
        ReadOnly = true,       // Only reads data, no modifications
        Secret = false,        // Returns non-sensitive information
        LocalRequired = false  // Pure cloud API calls
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceName.AsOptional());
        command.Validators.Add(commandResult =>
        {
            var namespaceName = commandResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceName.Name);
            var resourceGroup = commandResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

            if (!string.IsNullOrEmpty(namespaceName) && string.IsNullOrEmpty(resourceGroup))
            {
                commandResult.AddError("When specifying a namespace name, a resource group must also be provided.");
            }
        });
    }

    protected override NamespaceGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.NamespaceName = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceName.Name);
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
            var eventHubsService = context.GetService<IEventHubsService>();

            // Determine if this is a single namespace request or list request
            bool isSingleNamespaceRequest = !string.IsNullOrEmpty(options.NamespaceName);

            if (isSingleNamespaceRequest)
            {
                // Get single namespace with detailed information
                var namespaceDetails = await eventHubsService.GetNamespaceAsync(
                    options.NamespaceName!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = namespaceDetails != null
                    ? ResponseResult.Create(new(namespaceDetails), EventHubsJsonContext.Default.NamespaceGetCommandResult)
                    : null;
            }
            else
            {
                var namespaces = await eventHubsService.GetNamespacesAsync(
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = ResponseResult.Create(new(namespaces ?? []), EventHubsJsonContext.Default.NamespacesGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Event Hubs namespaces");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => $"Event Hubs namespace not found. Verify the namespace name, resource group, and that you have access.",
        Identity.AuthenticationFailedException authEx =>
            "Authentication failed. Please ensure your Azure credentials are properly configured and have not expired.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            "Access denied. Please ensure you have sufficient permissions to get Event Hubs namespaces in the specified resource group.",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "The specified resource group or subscription was not found. Please verify the resource group name and subscription.",
        ArgumentException argEx when argEx.ParamName == "resourceGroup" =>
            "Invalid resource group name. Please provide a valid resource group name.",
        ArgumentException argEx when argEx.ParamName == "subscription" =>
            "Invalid subscription. Please provide a valid subscription ID or name.",
        _ => base.GetErrorMessage(ex)
    };

    internal record NamespacesGetCommandResult(List<Models.Namespace> Namespaces);
    internal record NamespaceGetCommandResult(Models.Namespace Namespace);
}
