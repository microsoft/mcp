// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

[CommandMetadata(
    Id = "c1a8b3e5-4f2d-4a6e-8c7b-9d2e3f4a5b6c",
    Name = "get",
    Title = "Get Virtual Machine(s)",
    Description = "List or get all Azure Virtual Machines (VMs) in a subscription, or query VMs in a specific resource group. Show all VMs or retrieve a specific VM by name. Returns read-only VM details including name, location, VM size, provisioning state, OS type, and network interfaces. Use --instance-view to query and check the current runtime status and power state of a VM along with provisioning state. This is a read-only inspection and inventory tool for viewing VM configuration, properties, and runtime status.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VmGetCommand(ILogger<VmGetCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmGetOptions, VmGetCommand.VmGetResult>(subscriptionResolver)
{
    private readonly ILogger<VmGetCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override void ValidateOptions(VmGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Custom validation: If vm-name is specified, resource-group is required (can't get specific VM without resource-group)
        if (!string.IsNullOrEmpty(options.VmName) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            validationResult.Errors.Add("The --resource-group option is required when retrieving a specific VM with --vm-name.");
        }

        // Custom validation: If instance-view is specified, vm-name is required
        if (options.InstanceView && string.IsNullOrEmpty(options.VmName))
        {
            validationResult.Errors.Add("The --instance-view option is only available when retrieving a specific VM with --vm-name.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VmGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Scenario 1: Get specific VM with optional instance view
            if (!string.IsNullOrEmpty(options.VmName))
            {
                if (options.InstanceView)
                {
                    var vmWithInstanceView = await _computeService.GetVmWithInstanceViewAsync(
                        options.VmName,
                        options.ResourceGroup!,
                        options.Subscription!,
                        options.Tenant,
                        options.RetryPolicy,
                        cancellationToken);

                    context.Response.Results = ResponseResult.Create(
                        new(vmWithInstanceView.VmInfo, vmWithInstanceView.InstanceView, null),
                        ComputeJsonContext.Default.VmGetResult);
                }
                else
                {
                    var vm = await _computeService.GetVmAsync(
                        options.VmName,
                        options.ResourceGroup!,
                        options.Subscription!,
                        options.Tenant,
                        options.RetryPolicy,
                        cancellationToken);

                    context.Response.Results = ResponseResult.Create(new(vm, null, null), ComputeJsonContext.Default.VmGetResult);
                }
            }
            // Scenario 2: List VMs in resource group
            else
            {
                var vms = await _computeService.ListVmsAsync(
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, null, vms), ComputeJsonContext.Default.VmGetResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving VM(s). VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.VmName, options.ResourceGroup, options.Subscription);
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

    public sealed record VmGetResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] VmInfo? Vm,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] VmInstanceView? InstanceView,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<VmInfo>? Vms);
}
