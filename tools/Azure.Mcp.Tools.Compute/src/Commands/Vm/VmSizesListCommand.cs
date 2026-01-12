// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmSizesListCommand(ILogger<VmSizesListCommand> logger)
    : SubscriptionCommand<VmSizesListOptions>()
{
    private const string CommandTitle = "List Available VM Sizes";
    private readonly ILogger<VmSizesListCommand> _logger = logger;

    public override string Id => "f4d1e6h8-7i5g-7d9h-1f0e-2g5h6i7d8e9f";

    public override string Name => "sizes-list";

    public override string Description =>
        """
        Lists all available virtual machine sizes for a specified Azure region/location. Returns detailed information about each VM size including number of cores, memory in MB, max data disk count, OS disk size, and resource disk size.
        Use this command to discover available VM sizes when planning deployments or resizing VMs.
        Required parameters: subscription, location.
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
        command.Options.Add(ComputeOptionDefinitions.Location);
    }

    protected override VmSizesListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.LocationName);
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
            var computeService = context.GetService<IComputeService>();

            var sizes = await computeService.ListVmSizesAsync(
                options.Location!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(sizes ?? []), ComputeJsonContext.Default.VmSizesListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing VM sizes. Location: {Location}, Subscription: {Subscription}, Options: {@Options}",
                options.Location, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            "Invalid location specified. Verify the location/region name is correct.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing VM sizes. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmSizesListCommandResult(List<VmSizeInfo> Sizes);
}
