// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Id = "f330138e-8048-4a4a-8170-d8b6f958eaa4",
    Name = "update",
    Title = "Update Virtual Machine",
    Description = """
        Update, modify, or reconfigure an existing Azure Virtual Machine (VM) configuration.
        Use this to add or change tags on a VM, resize a VM to a different size, enable or configure boot diagnostics, or update user data.
        Equivalent to 'az vm update'. The VM may need to be deallocated before resizing to certain sizes.
        Do not use this to change VM power state (start, stop, deallocate, restart); use VM power-state instead.
        Do not use this to create a new VM (use VM create) or to update Virtual Machine Scale Sets (use VMSS update).
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VmUpdateCommand(ILogger<VmUpdateCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmUpdateOptions, VmUpdateCommand.VmUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VmUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    public override void ValidateOptions(VmUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Custom validation: At least one update property must be specified.
        // Note: Tags may be an empty string ("") to clear all tags — use GetResult to detect
        // whether the option was explicitly passed even with an empty value, since HasOptionResult
        // returns false for empty-string tokens.
        if (string.IsNullOrEmpty(options.VmSize) &&
            options.Tags == null &&
            string.IsNullOrEmpty(options.LicenseType) &&
            options.BootDiagnostics == null &&
            string.IsNullOrEmpty(options.UserData))
        {
            validationResult.Errors.Add("At least one update property must be specified: --vm-size, --tags, --license-type, --boot-diagnostics, or --user-data.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VmUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await _computeService.UpdateVmAsync(
                options.VmName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VmSize,
                options.Tags,
                options.LicenseType,
                options.BootDiagnostics,
                options.UserData,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(result), ComputeJsonContext.Default.VmUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating VM. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.VmName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "VM not found. Verify the VM name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to update VM. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Operation conflict. The VM may need to be deallocated for size changes. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) =>
            $"Quota exceeded. You may need to request a quota increase for the selected VM size. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VmUpdateCommandResult(VmUpdateResult Vm);
}
