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

public sealed class IoTHubCreateCommand(IIoTHubService service, ILogger<IoTHubCreateCommand> logger)
    : SubscriptionCommand<IoTHubCreateOptions>
{
    public override string Id => "iothub-create";
    public override string Name => "create";
    public override string Description => "Create a new IoT Hub.";
    public override string Title => "Create IoT Hub";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false };

    private readonly IIoTHubService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Location.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Sku.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Capacity.AsRequired());
    }

    protected override IoTHubCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Name.Name);
        options.Location = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Location.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Sku.Name);
        options.Capacity = parseResult.GetValueOrDefault<long>(IoTHubOptionDefinitions.Capacity.Name);
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
            var result = await _service.CreateIoTHub(
                options.Name!,
                options.ResourceGroup!,
                options.Location!,
                options.Sku!,
                options.Capacity!.Value,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.IoTHubDescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating IoT Hub {Name}", options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
