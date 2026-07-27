// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Options.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Disk;

/// <summary>
/// Command to delete an Azure managed disk.
/// </summary>
[CommandMetadata(
    Id = "a7c3e9f1-4b82-4d5a-9e6c-1f3d8b2a7c4e",
    Name = "delete",
    Title = "Delete Managed Disk",
    Description = "Deletes an Azure managed disk from the specified resource group. This is an idempotent operation that returns Deleted = true if the disk was successfully removed, or Deleted = false if the disk was not found. The disk must not be attached to a virtual machine; detach it first before deleting.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class DiskDeleteCommand(ILogger<DiskDeleteCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DiskDeleteOptions, DiskDeleteCommand.DiskDeleteCommandResult>(subscriptionResolver)
{
    private readonly ILogger<DiskDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DiskDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Deleting disk {DiskName} in resource group {ResourceGroup}",
                options.DiskName, options.ResourceGroup);

            var deleted = await _computeService.DeleteDiskAsync(
                options.DiskName,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(deleted, options.DiskName),
                ComputeJsonContext.Default.DiskDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting disk. Disk: {Disk}, ResourceGroup: {ResourceGroup}.",
                options.DiskName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "The disk is currently attached to a virtual machine. Detach the disk before deleting.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed deleting the disk. Details: {reqEx.Message}",
        Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx =>
            $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the disk delete command.
    /// </summary>
    public sealed record DiskDeleteCommandResult(bool Deleted, string DiskName);
}
