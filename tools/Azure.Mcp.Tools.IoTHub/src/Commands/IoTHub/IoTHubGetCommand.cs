// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.IoTHub;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.IoTHub;

public sealed class IoTHubGetCommand(IIoTHubService service, ILogger<IoTHubGetCommand> logger)
    : SubscriptionCommand<IoTHubGetOptions>
{
    public override string Id => "iothub-get";
    public override string Name => "get";
    public override string Description => "Get IoT Hub details or list all IoT Hubs in a subscription.";
    public override string Title => "Get IoT Hub";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    private readonly IIoTHubService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubOptionDefinitions.Name.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override IoTHubGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Name.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = await _service.GetIoTHub(
                options.Name,
                options.ResourceGroup,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.ListIoTHubDescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting IoT Hub(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
