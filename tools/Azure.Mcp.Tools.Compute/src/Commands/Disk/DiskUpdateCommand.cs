// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Disk;

/// <summary>
/// Command to update an Azure managed disk.
/// </summary>
[CommandMetadata(
    Id = "4a9b2c3d-6e7f-5b8c-9d0e-1f2a3b4c5d6e",
    Name = "update",
    Title = "Update Managed Disk",
    Description = "Updates or modifies properties of an existing Azure managed disk that was previously created. If resource group is not specified, the disk is located by name within the subscription. Supports changing disk size (can only increase), storage SKU, IOPS and throughput limits (UltraSSD only), max shares for shared disk attachments, on-demand bursting, tags, encryption settings, disk access, and performance tier. Modify the network access policy to DenyAll, AllowAll, or AllowPrivate on an existing disk. Only specified properties are updated; unspecified properties remain unchanged.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DiskUpdateCommand(ILogger<DiskUpdateCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DiskUpdateOptions, DiskUpdateCommand.DiskUpdateCommandResult>(subscriptionResolver)
{

    private readonly ILogger<DiskUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    public override void ValidateOptions(DiskUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.SizeGb == null &&
            string.IsNullOrEmpty(options.Sku) &&
            options.DiskIopsReadWrite == null &&
            options.DiskMbpsReadWrite == null &&
            options.MaxShares == null &&
            string.IsNullOrEmpty(options.NetworkAccessPolicy) &&
            options.EnableBursting == null &&
            options.Tags == null && // Tags have a special case where empty string means clear all tags
            string.IsNullOrEmpty(options.DiskEncryptionSet) &&
            string.IsNullOrEmpty(options.EncryptionType) &&
            string.IsNullOrEmpty(options.DiskAccess) &&
            string.IsNullOrEmpty(options.Tier))
            validationResult.Errors.Add("At least one update property must be provided "
                + "(size-gb, sku, disk-iops-read-write, disk-mbps-read-write, max-shares, "
                + "network-access-policy, enable-bursting, tags, disk-encryption-set, "
                + "encryption-type, disk-access-id, or tier).");
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DiskUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If resource group is not provided, search for the disk by name in the subscription
            if (string.IsNullOrEmpty(options.ResourceGroup))
            {
                _logger.LogInformation(
                    "Resource group not specified, searching for disk {DiskName} in subscription {Subscription}",
                    options.DiskName, options.Subscription);

                var disks = await _computeService.ListDisksAsync(
                    options.Subscription!,
                    null,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var matchingDisks = disks
                    .Where(d => string.Equals(d.Name, options.DiskName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingDisks.Count == 0)
                {
                    throw new ArgumentException($"Disk '{options.DiskName}' not found in subscription. Specify --resource-group to narrow the search.");
                }

                if (matchingDisks.Count > 1)
                {
                    var resourceGroups = string.Join(", ", matchingDisks.Select(d => d.ResourceGroup));
                    throw new ArgumentException(
                        $"Multiple disks named '{options.DiskName}' found in resource groups: {resourceGroups}. "
                        + "Specify --resource-group to disambiguate.");
                }

                var matchingDisk = matchingDisks[0];
                if (string.IsNullOrEmpty(matchingDisk.ResourceGroup))
                {
                    throw new ArgumentException($"Disk '{options.DiskName}' not found in subscription. Specify --resource-group to narrow the search.");
                }

                options.ResourceGroup = matchingDisk.ResourceGroup;
            }

            _logger.LogInformation(
                "Updating disk {DiskName} in resource group {ResourceGroup}",
                options.DiskName, options.ResourceGroup);

            var disk = await _computeService.UpdateDiskAsync(
                options.DiskName,
                options.ResourceGroup!,
                options.Subscription!,
                options.SizeGb,
                options.Sku,
                options.DiskIopsReadWrite,
                options.DiskMbpsReadWrite,
                options.MaxShares,
                options.NetworkAccessPolicy,
                options.EnableBursting,
                options.Tags,
                options.DiskEncryptionSet,
                options.EncryptionType,
                options.DiskAccess,
                options.Tier,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(disk), ComputeJsonContext.Default.DiskUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating disk. Disk: {Disk}, ResourceGroup: {ResourceGroup}.", options.DiskName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Disk not found. Verify the disk name and resource group are correct.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == 409 =>
            $"Conflict updating disk. The disk may be in use or the requested change is not allowed. Details: {reqEx.Message}",
        Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx =>
            $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the disk update command.
    /// </summary>
    public sealed record DiskUpdateCommandResult(DiskInfo Disk);
}
