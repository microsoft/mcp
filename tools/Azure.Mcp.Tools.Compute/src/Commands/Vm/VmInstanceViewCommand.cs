// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmInstanceViewCommand(ILogger<VmInstanceViewCommand> logger)
    : BaseComputeCommand<VmInstanceViewOptions>()
{
    private const string CommandTitle = "Get Virtual Machine Instance View";
    private readonly ILogger<VmInstanceViewCommand> _logger = logger;

    public override string Id => "e3c0d5g7-6h4f-6c8g-0e9d-1f4g5h6c7d8e";

    public override string Name => "instance-view";

    public override string Description =>
        """
        Retrieves the instance view of an Azure Virtual Machine with runtime information including power state, provisioning state, VM agent status, disk status, and extension status.
        Use this command to check the current operational status and health of a VM.
        Returns detailed runtime information including power state (running, stopped, deallocated) and component statuses.
        Required parameters: subscription, resource-group, vm-name.
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
        command.Options.Add(ComputeOptionDefinitions.VmName);
    }

    protected override VmInstanceViewOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmNameName);
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

            var instanceView = await computeService.GetVmInstanceViewAsync(
                options.VmName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(instanceView), ComputeJsonContext.Default.VmInstanceViewCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting VM instance view. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.VmName, options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine not found. Verify the VM name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the virtual machine. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmInstanceViewCommandResult(VmInstanceView InstanceView);
}
