// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssRollingUpgradeStatusCommand(ILogger<VmssRollingUpgradeStatusCommand> logger)
    : BaseComputeCommand<VmssRollingUpgradeStatusOptions>()
{
    private const string CommandTitle = "Get Scale Set Rolling Upgrade Status";
    private readonly ILogger<VmssRollingUpgradeStatusCommand> _logger = logger;

    public override string Id => "e9i6j1m3-2n0l-2i4m-6k5j-7l0m1n2i3j4k";

    public override string Name => "rolling-upgrade-status";

    public override string Description =>
        """
        Retrieves the status of rolling upgrade operations for a virtual machine scale set. Returns information including upgrade policy, running status with start time and last action, progress counters for successful/failed/in-progress/pending instances, and any error details.
        Use this command to monitor the progress and health of rolling upgrades on a scale set.
        Required parameters: subscription, resource-group, vmss-name.
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
        command.Options.Add(ComputeOptionDefinitions.VmssName);
    }

    protected override VmssRollingUpgradeStatusOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssNameName);
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

            var status = await computeService.GetVmssRollingUpgradeStatusAsync(
                options.VmssName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(status), ComputeJsonContext.Default.VmssRollingUpgradeStatusCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting VMSS rolling upgrade status. VmssName: {VmssName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.VmssName, options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine scale set or rolling upgrade not found. Verify the VMSS name, resource group, and that a rolling upgrade is in progress or completed.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the virtual machine scale set rolling upgrade status. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssRollingUpgradeStatusCommandResult(VmssRollingUpgradeStatus Status);
}
