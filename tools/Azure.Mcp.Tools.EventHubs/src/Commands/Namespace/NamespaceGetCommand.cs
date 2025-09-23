// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

public sealed class NamespaceGetCommand(ILogger<NamespaceGetCommand> logger)
    : BaseEventHubsCommand<NamespaceGetOptions>(logger)
{
    private const string CommandTitle = "Get Event Hubs Namespaces";

    public override string Name => "get";

    public override string Description =>
        """
        Get Event Hubs namespaces from Azure. This command can either:
        1. List all Event Hubs namespaces in a resource group (when only --resource-group is provided)
        2. Get a single namespace by name (using --namespace-name with --resource-group)
        
        When retrieving a single namespace, detailed information including SKU, settings, and metadata 
        is returned. When listing namespaces, basic information (name, id, resource group) is returned 
        for all namespaces in the specified resource group.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = true,      // Queries Azure resources - unpredictable domain
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
            bool isSingleNamespaceRequest = !string.IsNullOrEmpty(options.NamespaceName) && !string.IsNullOrEmpty(options.ResourceGroup);

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
                    ? ResponseResult.Create(new NamespaceGetSingleCommandResult(namespaceDetails), EventHubsJsonContext.Default.NamespaceGetSingleCommandResult)
                    : null;
            }
            else
            {
                // List all namespaces in resource group
                if (string.IsNullOrEmpty(options.ResourceGroup))
                {
                    // For backward compatibility, still require resource group for list operations
                    context.Response.Status = 400;
                    context.Response.Message = "Resource group is required when not specifying a specific namespace name.";
                    return context.Response;
                }

                var namespaces = await eventHubsService.GetNamespacesAsync(
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = namespaces?.Count > 0
                    ? ResponseResult.Create(new NamespaceGetCommandResult(namespaces), EventHubsJsonContext.Default.NamespaceGetCommandResult)
                    : null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Event Hubs namespaces");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => 404,
        RequestFailedException reqEx => reqEx.Status,
        Identity.AuthenticationFailedException => 401,
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

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

    internal record NamespaceGetCommandResult(List<Models.EventHubsNamespaceInfo> Namespaces);
    internal record NamespaceGetSingleCommandResult(Models.EventHubsNamespaceDetails Namespace);
}
