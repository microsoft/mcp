// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.SignalR.Options;
using Azure.Mcp.Tools.SignalR.Options.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.Runtime;

/// <summary>
/// Shows details of an Azure SignalR Service.
/// </summary>
public sealed class RuntimeGetCommand(ILogger<RuntimeGetCommand> logger)
    : BaseSignalRCommand<RuntimeGetOptions>
{
    private const string CommandTitle = "Show Service Details";
    private readonly ILogger<RuntimeGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Gets or lists details of an Azure SignalR Runtimes. If a specific SignalR name is used, the details of that
        SignalR runtime will be retrieved. Otherwise, all SignalR Runtimes in the specified subscription or resource
        group will be retrieved. Returns runtime information including identity, network ACLs, upstream templates.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SignalROptionDefinitions.SignalR);
    }

    protected override RuntimeGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SignalR ??= parseResult.GetValueOrDefault<string>(SignalROptionDefinitions.SignalR.Name);
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
            var runtimes = await signalRService.GetRuntimeAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.SignalR,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            _logger.LogInformation("Found {Count} SignalR service(s) in subscription {SubscriptionId}",
                runtimes.Count(), options.Subscription);

            context.Response.Results = ResponseResult.Create(new(runtimes ?? []), SignalRJsonContext.Default.RuntimeGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred showing SignalR service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RuntimeGetCommandResult(IEnumerable<Models.Runtime> Runtimes);
}
