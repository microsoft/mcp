// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Options;
using Azure.Mcp.Tools.SignalR.Options.Key;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.Key;

/// <summary>
/// Lists access keys for an Azure SignalR Service.
/// </summary>
public sealed class KeyListCommand(ILogger<KeyListCommand> logger)
    : BaseSignalRCommand<KeyListOptions>
{
    private const string CommandTitle = "List Access Keys";
    private readonly ILogger<KeyListCommand> _logger = logger;

    private static readonly Option<string> _signalRNameOption = SignalROptionDefinitions.SignalR;

    public override string Name => "list";

    public override string Description =>
        """
        List access keys for a SignalR service. This command retrieves and displays the primary and secondary
        access keys and connection strings for the specified SignalR service.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true, Secret = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_signalRNameOption);
    }

    protected override KeyListOptions BindOptions(ParseResult parseResult)
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
            var keys = await signalRService.ListKeysAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.SignalR!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = keys is null
                ? null
                : ResponseResult.Create(
                    new KeyListCommandResult(keys),
                    SignalRJsonContext.Default.KeyListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing SignalR access keys");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KeyListCommandResult(Models.Key Keys);
}
