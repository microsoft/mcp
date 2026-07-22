// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

[CommandMetadata(
    Id = "d4e2c8a1-6f3b-4d9e-b8c7-1a2e3f4d5e6f",
    Name = "delete",
    Title = "Delete Virtual Machine",
    Description = """
        Delete, remove, or destroy an Azure Virtual Machine (VM) permanently.
        Use this only when the VM is no longer needed and should be permanently removed.
        Equivalent to 'az vm delete'. This operation is irreversible and the VM data will be lost.
        Use --force-deletion to force delete the VM even if it is in a running or failed state
        (passes forceDeletion=true to the Azure API).
        Associated resources like disks, NICs, and public IPs are NOT automatically deleted.
        Do not use this to stop or pause a VM; use the VM power-state command instead.
        Do not use this to delete Virtual Machine Scale Sets (use VMSS delete instead).
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class VmDeleteCommand(ILogger<VmDeleteCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmDeleteOptions, VmDeleteCommand.VmDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VmDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VmDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var deleted = await _computeService.DeleteVmAsync(
                options.VmName,
                options.ResourceGroup,
                options.Subscription!,
                options.ForceDeletion ? true : null,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var message = deleted
                ? $"Virtual machine '{options.VmName}' was successfully deleted from resource group '{options.ResourceGroup}'."
                : $"Virtual machine '{options.VmName}' was not found in resource group '{options.ResourceGroup}'. Nothing was deleted.";

            context.Response.Results = ResponseResult.Create(new(message, deleted), ComputeJsonContext.Default.VmDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting VM. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.VmName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to delete the VM. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Operation conflict. The VM may be in a state that prevents deletion. Try using --force-deletion. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VmDeleteCommandResult(string Message, bool Success);
}
