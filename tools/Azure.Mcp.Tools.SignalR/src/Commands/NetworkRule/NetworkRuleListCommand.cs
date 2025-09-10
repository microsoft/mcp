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

    private readonly Option<string> _signalRNameOption = SignalROptionDefinitions.SignalR;

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
        command.Options.Add(_signalRNameOption);
    }

    protected override NetworkRuleListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SignalR = parseResult.GetValue(_signalRNameOption);
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
            var service = context.GetService<ISignalRService>();

            var networkRules = await service.GetNetworkRulesAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.SignalR!,
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
                "Error listing network rules for SignalR service. SignalR: {SignalR}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.SignalR, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record NetworkRuleListCommandResult(Models.NetworkRule NetworkRules);
}
