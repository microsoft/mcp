// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmGetCommand(ILogger<VmGetCommand> logger)
    : BaseComputeCommand<VmGetOptions>()
{
    private const string CommandTitle = "Get Virtual Machine(s)";
    private readonly ILogger<VmGetCommand> _logger = logger;

    public override string Id => "c1a8b3e5-4f2d-4a6e-8c7b-9d2e3f4a5b6c";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves information about Azure Virtual Machine(s). Behavior depends on provided parameters:
        - With --vm-name: Gets detailed information about a specific VM (requires --resource-group). Optionally include --instance-view for runtime status.
        - With --resource-group only: Lists all VMs in the specified resource group.
        - With neither: Lists all VMs in the subscription.
        Returns VM information including name, location, VM size, provisioning state, OS type, license type, zones, and tags.
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

        // Make resource-group optional for listing scenarios
        command.Options.Remove(OptionDefinitions.Common.ResourceGroup);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());

        // Add optional vm-name
        command.Options.Add(ComputeOptionDefinitions.VmName);

        // Add optional instance-view
        command.Options.Add(ComputeOptionDefinitions.InstanceView);
    }

    protected override VmGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
        options.InstanceView = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.InstanceView.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        // Custom validation: If vm-name is specified, resource-group is required
        if (!string.IsNullOrEmpty(options.VmName) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "When --vm-name is specified, --resource-group is required.";
            return context.Response;
        }

        // Custom validation: If instance-view is specified, vm-name is required
        if (options.InstanceView && string.IsNullOrEmpty(options.VmName))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The --instance-view option is only available when retrieving a specific VM with --vm-name.";
            return context.Response;
        }
        var computeService = context.GetService<IComputeService>();

        try
        {
            // Scenario 1: Get specific VM with optional instance view
            if (!string.IsNullOrEmpty(options.VmName))
            {
                if (options.InstanceView)
                {
                    var vmWithInstanceView = await computeService.GetVmWithInstanceViewAsync(
                        options.VmName,
                        options.ResourceGroup!,
                        options.Subscription!,
                        options.Tenant,
                        options.RetryPolicy,
                        cancellationToken);

                    context.Response.Results = ResponseResult.Create(
                        new VmGetSingleResult(vmWithInstanceView.VmInfo, vmWithInstanceView.InstanceView),
                        ComputeJsonContext.Default.VmGetSingleResult);
                }
                else
                {
                    var vm = await computeService.GetVmAsync(
                        options.VmName,
                        options.ResourceGroup!,
                        options.Subscription!,
                        options.Tenant,
                        options.RetryPolicy,
                        cancellationToken);

                    context.Response.Results = ResponseResult.Create(
                        new VmGetSingleResult(vm, null),
                        ComputeJsonContext.Default.VmGetSingleResult);
                }
            }
            // Scenario 2 & 3: List VMs (in resource group or subscription)
            else
            {
                var vms = await computeService.ListVmsAsync(
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new VmGetListResult(vms),
                    ComputeJsonContext.Default.VmGetListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving VM(s). VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
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
            $"Authorization failed accessing virtual machine(s). Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmGetSingleResult(VmInfo Vm, VmInstanceView? InstanceView);
    internal record VmGetListResult(List<VmInfo> Vms);
}
