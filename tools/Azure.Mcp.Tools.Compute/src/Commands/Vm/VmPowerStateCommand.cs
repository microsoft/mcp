// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

[CommandMetadata(
    Id = "a7c1e4b2-9d3f-4e8a-b5c6-2f1d3e4a5b6c",
    Name = "power-state",
    Description =
        """
        Deallocate, start, stop, power on, power off, or restart an Azure Virtual Machine (VM) to change its running state.
        Use this to deallocate a VM to release compute resources while keeping the VM and preserving its configuration,
        power on a stopped VM, power off or shut down a running VM, or reboot a VM.
        Deallocating a VM stops billing for compute resources while the VM remains available to be started again later.
        Supported --power-action values: deallocate (release compute resources while keeping the VM), start (power on), stop (power off, shut down), restart (reboot).
        Equivalent to 'az vm deallocate', 'az vm start', 'az vm stop', 'az vm restart'.
        Use --skip-shutdown with stop to force the action without graceful OS shutdown.
        Use --no-wait to return immediately; the response will include a 'statusUri' (the ARM Azure-AsyncOperation URL)
        that can be GET'd to poll the operation status (InProgress / Succeeded / Failed).
        Do not use this to query, check, or get a VM's current state; use the VM get command with --instance-view instead.
        """,
    Title = "Change Virtual Machine Power State",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class VmPowerStateCommand(ILogger<VmPowerStateCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmPowerStateOptions>(true)
{
    private static readonly HashSet<string> s_validActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "start", "stop", "deallocate", "restart"
    };
    private readonly ILogger<VmPowerStateCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(ComputeOptionDefinitions.VmName.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.PowerAction.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.NoWait);
        command.Options.Add(ComputeOptionDefinitions.SkipShutdown);

        command.Validators.Add(commandResult =>
        {
            var powerAction = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.PowerAction.Name);
            if (!string.IsNullOrEmpty(powerAction) && !s_validActions.Contains(powerAction))
            {
                commandResult.AddError($"Invalid --power-action value '{powerAction}'. Accepted values: start, stop, deallocate, restart.");
            }

            var skipShutdown = commandResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.SkipShutdown.Name);
            if (skipShutdown && !string.Equals(powerAction, "stop", StringComparison.OrdinalIgnoreCase))
            {
                commandResult.AddError("--skip-shutdown is only compatible with --power-action stop.");
            }
        });
    }

    protected override VmPowerStateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
        options.PowerAction = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.PowerAction.Name);
        options.NoWait = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.NoWait.Name);
        options.SkipShutdown = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.SkipShutdown.Name);
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
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await _computeService.ChangeVmPowerStateAsync(
                options.VmName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.PowerAction!,
                options.NoWait,
                options.SkipShutdown,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                ComputeJsonContext.Default.VmPowerStateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error changing VM power state. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, PowerAction: {PowerAction}",
                options.VmName, options.ResourceGroup, options.Subscription, options.PowerAction);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "VM not found. Verify the VM name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to change the VM power state. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Operation conflict. The VM may be in a state that prevents this power operation. Details: {reqEx.Message}",
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmPowerStateCommandResult(Models.VmPowerStateResult PowerState);
}
