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

public sealed class NamespaceDeleteCommand(ILogger<NamespaceDeleteCommand> logger)
    : BaseEventHubsCommand<NamespaceDeleteOptions>
{
    private const string CommandTitle = "Delete Event Hubs Namespace";

    private readonly ILogger<NamespaceDeleteCommand> _logger = logger;

    public override string Name => "delete";

    public override string Description =>
        """
        Delete a Namespace. This tool will delete a pre-existing Namespace from the 
        specified resource group. This tool will remove existing configurations, and is 
        considered to be destructive.
        
        WARNING: This operation is irreversible. All Event Hubs, consumer groups, and 
        configurations within the namespace will be permanently deleted.
        
        The namespace must exist in the specified resource group. If the namespace is not found,
        an error will be returned.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,    // Permanently deletes resources
        Idempotent = true,     // Deleting same resource multiple times has same effect
        ReadOnly = false,      // Modifies cloud state
        Secret = false,        // Returns non-sensitive information
        LocalRequired = false  // Pure cloud API calls
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
    }

    protected override NamespaceDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.Namespace);
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

            var success = await eventHubsService.DeleteNamespaceAsync(
                options.Namespace!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new NamespaceDeleteCommandResult(success, $"Namespace '{options.Namespace}' deleted successfully"),
                EventHubsJsonContext.Default.NamespaceDeleteCommandResult);
            context.Response.Status = HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Event Hubs namespace '{NamespaceName}' from resource group '{ResourceGroup}'",
                options.Namespace, options.ResourceGroup);
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
            "Access denied. Please ensure you have sufficient permissions to delete Event Hubs namespaces in the specified resource group.",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "The specified resource group, subscription, or namespace was not found. Please verify the names.",
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "Conflict: The namespace cannot be deleted in its current state. It may be in the process of being updated or deleted.",
        ArgumentException argEx when argEx.ParamName == "resourceGroup" =>
            "Invalid resource group name. Please provide a valid resource group name.",
        ArgumentException argEx when argEx.ParamName == "subscription" =>
            "Invalid subscription. Please provide a valid subscription ID or name.",
        _ => base.GetErrorMessage(ex)
    };

    internal record NamespaceDeleteCommandResult(bool Success, string Message);
}
