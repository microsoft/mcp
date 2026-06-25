// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
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
/// Command to create an Azure managed disk.
/// </summary>
[CommandMetadata(
    Id = "3f8a1b2c-5d6e-4a7b-8c9d-0e1f2a3b4c5d",
    Name = "create",
    Title = "Create Managed Disk",
    Description = "Creates a new Azure managed disk in the specified resource group. Supports creating empty disks (specify --size-gb), disks from a source such as a snapshot, another managed disk, or a blob URI (specify --source), disks from a Shared Image Gallery image version (specify --gallery-image-reference), or disks ready for upload (specify --upload-type and --upload-size-bytes). If location is not specified, defaults to the resource group's location. Supports configuring disk size, storage SKU (e.g., Premium_LRS, Standard_LRS, UltraSSD_LRS), OS type, availability zone, hypervisor generation, tags, encryption settings, performance tier, shared disk, on-demand bursting, and IOPS/throughput limits for UltraSSD disks. Create a disk with network access policy DenyAll, AllowAll, or AllowPrivate and associate a disk access resource during creation.",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DiskCreateCommand(ILogger<DiskCreateCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DiskCreateOptions, DiskCreateCommand.DiskCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<DiskCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    public override void ValidateOptions(DiskCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.Source) &&
            (options.SizeGb == null || options.SizeGb <= 0) &&
            string.IsNullOrEmpty(options.GalleryImageReference) &&
            string.IsNullOrEmpty(options.UploadType))
        {
            validationResult.Errors.Add("Either --source, --size-gb, --gallery-image-reference, or --upload-type must be specified.");
        }

        if (!string.IsNullOrEmpty(options.UploadType) &&
            (options.UploadSizeBytes == null || options.UploadSizeBytes <= 0))
        {
            validationResult.Errors.Add("--upload-size-bytes is required when --upload-type is specified.");
        }

        if (string.Equals(options.UploadType, "UploadWithSecurityData", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrEmpty(options.SecurityType))
        {
            validationResult.Errors.Add("--security-type is required when --upload-type is 'UploadWithSecurityData'.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DiskCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating disk {DiskName} in resource group {ResourceGroup}, location {Location}, source {Source}",
                options.DiskName, options.ResourceGroup, options.Location ?? "(default)", options.Source ?? "(none)");

            var disk = await _computeService.CreateDiskAsync(
                options.DiskName,
                options.ResourceGroup,
                options.Subscription!,
                options.Source,
                options.Location,
                options.SizeGb,
                options.Sku,
                options.OsType,
                options.Zone,
                options.HyperVGeneration,
                options.MaxShares,
                options.NetworkAccessPolicy,
                options.EnableBursting,
                options.Tags,
                options.DiskEncryptionSet,
                options.EncryptionType,
                options.DiskAccess,
                options.Tier,
                options.GalleryImageReference,
                options.GalleryImageReferenceLun,
                options.DiskIopsReadWrite,
                options.DiskMbpsReadWrite,
                options.UploadType,
                options.UploadSizeBytes,
                options.SecurityType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(disk), ComputeJsonContext.Default.DiskCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating disk. Disk: {Disk}, ResourceGroup: {ResourceGroup}.", options.DiskName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        SecurityException => HttpStatusCode.BadRequest,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "A disk with this name already exists in the resource group.",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Resource group not found. Verify the resource group name is correct.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed. Details: {reqEx.Message}",
        Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        SecurityException secEx =>
            $"Invalid parameter: {secEx.Message}",
        ArgumentException argEx =>
            $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the disk create command.
    /// </summary>
    public record DiskCreateCommandResult(DiskInfo Disk);
}
