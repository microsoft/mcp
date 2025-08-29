// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Options;
using Azure.Mcp.Tools.SignalR.Options.NetworkRule;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.NetworkRule;

/// <summary>
/// Command to list SignalR network rules.
/// </summary>
public sealed class NetworkRuleListCommand(ILogger<NetworkRuleListCommand> logger)
    : BaseSignalRCommand<NetworkRuleListOptions>
{
    private const string CommandTitle = "List SignalR Network Rules";
    private readonly ILogger<NetworkRuleListCommand> _logger = logger;

    private readonly Option<string> _signalRNameOption = SignalROptionDefinitions.SignalRName;

    public override string Name => "list";

    public override string Description =>
        """
        Lists network access control rules for a SignalR service.
        Returns the network ACL configuration including public network rules and private endpoint rules.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.AddOption(_signalRNameOption);
    }

    protected override NetworkRuleListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SignalRName = parseResult.GetValueForOption(_signalRNameOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            // Required validation step
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<ISignalRService>();

            var networkRules = await service.GetNetworkRulesAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.SignalRName!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = networkRules is null
                ? null
                : ResponseResult.Create(
                    new NetworkRuleListCommandResult(networkRules),
                    SignalRJsonContext.Default.NetworkRuleListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing network rules for SignalR service. SignalR: {SignalRName}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.SignalRName, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "SignalR service not found. Verify the service name, resource group, and subscription are correct.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the SignalR service. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    /// <summary>
    /// Result for the network rule list command.
    /// </summary>
    internal record NetworkRuleListCommandResult(Models.NetworkRule NetworkRules);
}
