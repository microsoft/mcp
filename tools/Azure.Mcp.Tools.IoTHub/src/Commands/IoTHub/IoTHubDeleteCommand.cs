// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.IoTHub;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.IoTHub;

public sealed class IoTHubDeleteCommand(IIoTHubService service, ILogger<IoTHubDeleteCommand> logger)
    : SubscriptionCommand<IoTHubDeleteOptions>
{
    public override string Id => "iothub-delete";
    public override string Name => "delete";
    public override string Description => "Delete an IoT Hub.";
    public override string Title => "Delete IoT Hub";
    public override ToolMetadata Metadata => new() { Destructive = true, Idempotent = true, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = false };

    private readonly IIoTHubService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    }

    protected override IoTHubDeleteOptions BindOptions(ParseResult parseResult)
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
            await _service.DeleteIoTHub(
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new IoTHubDeleteCommandResult("IoT Hub deleted successfully."),
                IoTHubJsonContext.Default.IoTHubDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting IoT Hub {Name}", options.Name);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

public record IoTHubDeleteCommandResult(string Message);
