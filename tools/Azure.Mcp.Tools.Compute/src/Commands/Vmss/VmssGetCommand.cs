// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

[CommandMetadata(
    Id = "a5e2f7i9-8j6h-8e0i-2g1f-3h6i7j8e9f0g",
    Name = "get",
    Title = "Get Virtual Machine Scale Set(s)",
    Description = "List, show, or get Azure Virtual Machine Scale Sets (VMSS) and their instances in a subscription or resource group. Show all scale sets or get a specific VMSS by name. Get VMSS instance details by instance ID. Returns scale set details including name, location, SKU, capacity, upgrade policy, and individual VM instance information. Do not use this for single standalone VMs (use VM get instead).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VmssGetCommand(ILogger<VmssGetCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmssGetOptions, VmssGetCommand.VmssGetResult>(subscriptionResolver)
{
    private readonly ILogger<VmssGetCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override void ValidateOptions(VmssGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Custom validation: If vmss-name is specified, resource-group is required (can't get specific VMSS without resource-group)
        if (!string.IsNullOrEmpty(options.VmssName) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            validationResult.Errors.Add("The --resource-group option is required when retrieving a specific VMSS with --vmss-name.");
        }

        // Custom validation: If instance-id is specified, vmss-name is required
        if (!string.IsNullOrEmpty(options.InstanceId) && string.IsNullOrEmpty(options.VmssName))
        {
            validationResult.Errors.Add("When --instance-id is specified, --vmss-name is required.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VmssGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Scenario 1: Get specific VM instance in VMSS
            if (!string.IsNullOrEmpty(options.InstanceId))
            {
                var vmInstance = await _computeService.GetVmssVmAsync(
                    options.VmssName!,
                    options.InstanceId,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, vmInstance, null), ComputeJsonContext.Default.VmssGetResult);
            }
            // Scenario 2: Get specific VMSS
            else if (!string.IsNullOrEmpty(options.VmssName))
            {
                var vmss = await _computeService.GetVmssAsync(
                    options.VmssName,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(vmss, null, null), ComputeJsonContext.Default.VmssGetResult);
            }
            // Scenario 3: List VMSS in resource group
            else
            {
                var vmssList = await _computeService.ListVmssAsync(
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, null, vmssList ?? []), ComputeJsonContext.Default.VmssGetResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving VMSS. VmssName: {VmssName}, InstanceId: {InstanceId}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.VmssName, options.InstanceId, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine scale set or instance not found. Verify the VMSS name, instance ID, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing virtual machine scale set(s). Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VmssGetResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] VmssInfo? Vmss,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] VmssVmInfo? VmInstance,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<VmssInfo>? VmssList);
}
