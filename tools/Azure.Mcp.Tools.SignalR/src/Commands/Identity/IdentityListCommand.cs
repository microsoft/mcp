// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Options;
using Azure.Mcp.Tools.SignalR.Options.Identity;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.Identity;

/// <summary>
/// List the managed identity configuration of an Azure SignalR Service.
/// </summary>
public sealed class IdentityListCommand(ILogger<IdentityListCommand> logger)
    : BaseSignalRCommand<IdentityListOptions>
{
    private const string CommandTitle = "List Identity Configuration";
    private readonly ILogger<IdentityListCommand> _logger = logger;

    private static readonly Option<string> _signalRNameOption = SignalROptionDefinitions.SignalR;

    public override string Name => "list";

    public override string Description =>
        """
        List the managed identity configuration of an Azure SignalR Service. Returns identity information
        including type (SystemAssigned, UserAssigned), principal ID, tenant ID, and any
        user-assigned identities associated with the service.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true, Secret = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_signalRNameOption);
    }

    protected override IdentityListOptions BindOptions(ParseResult parseResult)
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
            var signalRService = context.GetService<ISignalRService>();
            var identity = await signalRService.GetSignalRIdentityAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.SignalR!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = identity is null
                ? null
                : ResponseResult.Create(
                    new IdentityListCommandResult(identity),
                    SignalRJsonContext.Default.IdentityListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing SignalR identity");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record IdentityListCommandResult(Models.Identity Identity);
}
