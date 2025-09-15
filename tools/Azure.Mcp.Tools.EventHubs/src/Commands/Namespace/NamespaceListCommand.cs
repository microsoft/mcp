// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.Namespace;

public sealed class NamespaceListCommand(ILogger<NamespaceListCommand> logger)
    : BaseEventHubsCommand<NamespaceListOptions>(logger)
{
    private const string CommandTitle = "List EventHubs Namespaces";

    public override string Name => "list";

    public override string Description =>
        """
        List all EventHubs namespaces in a resource group. This command retrieves all EventHubs namespaces 
        available in the specified resource group and subscription. Results are returned as a JSON array 
        of objects containing the name, id, and resource group of each namespace.
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
        RequireResourceGroup(); // Resource group is required for this command
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
            var namespaces = await eventHubsService.ListNamespacesAsync(
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = namespaces?.Count > 0
                ? ResponseResult.Create(new NamespaceListCommandResult(namespaces), EventHubsJsonContext.Default.NamespaceListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing EventHubs namespaces");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        Azure.Identity.AuthenticationFailedException => 401,
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.Identity.AuthenticationFailedException authEx =>
            "Authentication failed. Please ensure your Azure credentials are properly configured and have not expired.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            "Access denied. Please ensure you have sufficient permissions to list EventHubs namespaces in the specified resource group.",
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "The specified resource group or subscription was not found. Please verify the resource group name and subscription.",
        ArgumentException argEx when argEx.ParamName == "resourceGroup" =>
            "Invalid resource group name. Please provide a valid resource group name.",
        ArgumentException argEx when argEx.ParamName == "subscription" =>
            "Invalid subscription. Please provide a valid subscription ID or name.",
        _ => base.GetErrorMessage(ex)
    };
    internal record NamespaceListCommandResult(List<Models.EventHubsNamespaceInfo> Namespaces);
}
