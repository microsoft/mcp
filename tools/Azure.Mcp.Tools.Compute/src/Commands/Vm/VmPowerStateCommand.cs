// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmPowerStateCommand(ILogger<VmPowerStateCommand> logger)
    : BaseComputeCommand<VmPowerStateOptions>(true)
{
    private const string CommandTitle = "Change Virtual Machine Power State";
    private static readonly string[] s_validStates = ["start", "stop", "deallocate", "restart"];
    private readonly ILogger<VmPowerStateCommand> _logger = logger;

    public override string Id => "a7c1e4b2-9d3f-4e8a-b5c6-2f1d3e4a5b6c";

    public override string Name => "power-state";

    public override string Description =>
        """
        Start, stop, deallocate, or restart an Azure VM via --state.
        Equivalent to 'az vm start/stop/deallocate/restart'.
        Use --skip-shutdown with stop to force power off. Use --no-wait to return immediately.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(ComputeOptionDefinitions.VmName.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.State.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.NoWait);
        command.Options.Add(ComputeOptionDefinitions.SkipShutdown);

        command.Validators.Add(commandResult =>
        {
            var state = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.State.Name);
            if (!string.IsNullOrEmpty(state) && !s_validStates.Contains(state, StringComparer.OrdinalIgnoreCase))
            {
                commandResult.AddError($"Invalid --state value '{state}'. Accepted values: start, stop, deallocate, restart.");
            }

            var skipShutdown = commandResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.SkipShutdown.Name);
            if (skipShutdown && !string.Equals(state, "stop", StringComparison.OrdinalIgnoreCase))
            {
                commandResult.AddError("--skip-shutdown is only compatible with --state stop.");
            }
        });
    }

    protected override VmPowerStateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
        options.State = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.State.Name);
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

        var computeService = context.GetService<IComputeService>();

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await computeService.ChangeVmPowerStateAsync(
                options.VmName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.State!,
                options.NoWait,
                options.SkipShutdown,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VmPowerStateCommandResult(result),
                ComputeJsonContext.Default.VmPowerStateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error changing VM power state. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, State: {State}",
                options.VmName, options.ResourceGroup, options.Subscription, options.State);
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

    internal record VmPowerStateCommandResult(Models.VmPowerStateResult Result);
}
