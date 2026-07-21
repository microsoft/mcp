// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Id = "aaa0ad51-3c16-4ec2-99e2-b24f28a1e7d0",
    Name = "update",
    Title = "Update Virtual Machine Scale Set",
    Description = """
        Update, modify, or reconfigure an existing Azure Virtual Machine Scale Set (VMSS).
        Use this only on a VMSS that already exists to adjust its instance count, resize its VMs,
        switch its upgrade policy, or update its tags. Equivalent to 'az vmss update'.
        Changes may require 'update-instances' to roll out to existing VMs.
        Do not use this to create, deploy, or provision a new VMSS (use VMSS create instead) or to update a single VM (use VM update).
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VmssUpdateCommand(ILogger<VmssUpdateCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmssUpdateOptions, VmssUpdateCommand.VmssUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VmssUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    public override void ValidateOptions(VmssUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Custom validation: At least one update property must be specified
        if (string.IsNullOrEmpty(options.UpgradePolicy) &&
            options.Capacity == null &&
            string.IsNullOrEmpty(options.VmSize) &&
            options.Overprovision == null &&
            options.EnableAutoOsUpgrade == null &&
            string.IsNullOrEmpty(options.ScaleInPolicy) &&
            options.Tags == null)
        {
            validationResult.Errors.Add(
                "At least one update property must be specified: --upgrade-policy, --capacity, --vm-size, --overprovision, --enable-auto-os-upgrade, --scale-in-policy, or --tags.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VmssUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await _computeService.UpdateVmssAsync(
                options.VmssName,
                options.ResourceGroup,
                options.Subscription!,
                options.VmSize,
                options.Capacity,
                options.UpgradePolicy,
                options.Overprovision,
                options.EnableAutoOsUpgrade,
                options.ScaleInPolicy,
                options.Tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(result), ComputeJsonContext.Default.VmssUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating VMSS. VmssName: {VmssName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.VmssName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "VMSS not found. Verify the VMSS name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to update VMSS. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) =>
            $"Quota exceeded. You may need to request a quota increase for the selected VM size or capacity. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VmssUpdateCommandResult(VmssUpdateResult Vmss);
}
