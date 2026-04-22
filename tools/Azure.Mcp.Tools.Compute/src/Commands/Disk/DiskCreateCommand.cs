// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Disk;

/// <summary>
/// Command to create an Azure managed disk.
/// </summary>
public sealed class DiskCreateCommand(
    ILogger<DiskCreateCommand> logger)
    : BaseComputeCommand<DiskCreateOptions>(true)
{
    private const string CommandTitle = "Create Managed Disk";
    private const string CommandDescription =
        "Creates a new Azure managed disk in the specified resource group. "
        + "Supports creating empty disks (specify --size-gb), disks from a source such as a snapshot, "
        + "another managed disk, or a blob URI (specify --source), disks from a Shared Image Gallery "
        + "image version (specify --gallery-image-reference), or disks ready for upload "
        + "(specify --upload-type and --upload-size-bytes). "
        + "If location is not specified, defaults to the resource group's location. "
        + "Supports configuring disk size, storage SKU (e.g., Premium_LRS, Standard_LRS, UltraSSD_LRS), "
        + "OS type, availability zone, hypervisor generation, tags, encryption settings, "
        + "performance tier, shared disk, on-demand bursting, "
        + "and IOPS/throughput limits for UltraSSD disks. "
        + "Create a disk with network access policy DenyAll, AllowAll, or AllowPrivate "
        + "and associate a disk access resource during creation.";

    private readonly ILogger<DiskCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Id => "3f8a1b2c-5d6e-4a7b-8c9d-0e1f2a3b4c5d";

    public override string Name => "create";

    public override string Title => CommandTitle;

    public override string Description => CommandDescription;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,
        Idempotent = false,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Disk.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Source);
        command.Options.Add(ComputeOptionDefinitions.Location);
        command.Options.Add(ComputeOptionDefinitions.SizeGb);
        command.Options.Add(ComputeOptionDefinitions.Sku);
        command.Options.Add(ComputeOptionDefinitions.OsType);
        command.Options.Add(ComputeOptionDefinitions.Zone);
        command.Options.Add(ComputeOptionDefinitions.HyperVGeneration);
        command.Options.Add(ComputeOptionDefinitions.MaxShares);
        command.Options.Add(ComputeOptionDefinitions.NetworkAccessPolicy);
        command.Options.Add(ComputeOptionDefinitions.EnableBursting);
        command.Options.Add(ComputeOptionDefinitions.Tags);
        command.Options.Add(ComputeOptionDefinitions.DiskEncryptionSet);
        command.Options.Add(ComputeOptionDefinitions.EncryptionType);
        command.Options.Add(ComputeOptionDefinitions.DiskAccessId);
        command.Options.Add(ComputeOptionDefinitions.Tier);
        command.Options.Add(ComputeOptionDefinitions.GalleryImageReference);
        command.Options.Add(ComputeOptionDefinitions.GalleryImageReferenceLun);
        command.Options.Add(ComputeOptionDefinitions.DiskIopsReadWrite);
        command.Options.Add(ComputeOptionDefinitions.DiskMbpsReadWrite);
        command.Options.Add(ComputeOptionDefinitions.UploadType);
        command.Options.Add(ComputeOptionDefinitions.UploadSizeBytes);
        command.Options.Add(ComputeOptionDefinitions.SecurityType);
        command.Validators.Add(result =>
        {
            var source = result.GetValueOrDefault(ComputeOptionDefinitions.Source);
            var sizeGb = result.GetValueOrDefault(ComputeOptionDefinitions.SizeGb);
            var galleryImageReference = result.GetValueOrDefault(ComputeOptionDefinitions.GalleryImageReference);
            var uploadType = result.GetValueOrDefault(ComputeOptionDefinitions.UploadType);
            var uploadSizeBytes = result.GetValueOrDefault(ComputeOptionDefinitions.UploadSizeBytes);
            var securityType = result.GetValueOrDefault(ComputeOptionDefinitions.SecurityType);

            if (string.IsNullOrEmpty(source) && sizeGb <= 0 && string.IsNullOrEmpty(galleryImageReference) && string.IsNullOrEmpty(uploadType))
            {
                result.AddError("Either --source, --size-gb, --gallery-image-reference, or --upload-type must be specified.");
            }

            if (!string.IsNullOrEmpty(uploadType) && uploadSizeBytes <= 0)
            {
                result.AddError("--upload-size-bytes is required when --upload-type is specified.");
            }

            if (string.Equals(uploadType, "UploadWithSecurityData", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrEmpty(securityType))
            {
                result.AddError("--security-type is required when --upload-type is 'UploadWithSecurityData'.");
            }
        });
    }

    protected override DiskCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Disk = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Disk);
        options.Source = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Source);
        options.Location = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Location);
        options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);

        var sizeGb = parseResult.GetValueOrDefault(ComputeOptionDefinitions.SizeGb);
        options.SizeGb = sizeGb > 0 ? sizeGb : null;

        options.Sku = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Sku);
        options.OsType = parseResult.GetValueOrDefault(ComputeOptionDefinitions.OsType);
        options.Zone = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Zone);
        options.HyperVGeneration = parseResult.GetValueOrDefault(ComputeOptionDefinitions.HyperVGeneration);

        var maxShares = parseResult.GetValueOrDefault(ComputeOptionDefinitions.MaxShares);
        options.MaxShares = maxShares > 0 ? maxShares : null;

        options.NetworkAccessPolicy = parseResult.GetValueOrDefault(ComputeOptionDefinitions.NetworkAccessPolicy);
        options.EnableBursting = parseResult.GetValueOrDefault(ComputeOptionDefinitions.EnableBursting);
        options.Tags = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Tags);
        options.DiskEncryptionSet = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskEncryptionSet);
        options.EncryptionType = parseResult.GetValueOrDefault(ComputeOptionDefinitions.EncryptionType);
        options.DiskAccessId = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskAccessId);
        options.Tier = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Tier);
        options.GalleryImageReference = parseResult.GetValueOrDefault(ComputeOptionDefinitions.GalleryImageReference);

        options.GalleryImageReferenceLun = parseResult.GetValueOrDefault(ComputeOptionDefinitions.GalleryImageReferenceLun);

        var iops = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskIopsReadWrite);
        options.DiskIopsReadWrite = iops > 0 ? iops : null;

        var mbps = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskMbpsReadWrite);
        options.DiskMbpsReadWrite = mbps > 0 ? mbps : null;

        options.UploadType = parseResult.GetValueOrDefault(ComputeOptionDefinitions.UploadType);

        var uploadSizeBytes = parseResult.GetValueOrDefault(ComputeOptionDefinitions.UploadSizeBytes);
        options.UploadSizeBytes = uploadSizeBytes > 0 ? uploadSizeBytes : null;

        options.SecurityType = parseResult.GetValueOrDefault(ComputeOptionDefinitions.SecurityType);

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
            _logger.LogInformation(
                "Creating disk {DiskName} in resource group {ResourceGroup}, location {Location}, source {Source}",
                options.Disk, options.ResourceGroup, options.Location ?? "(default)", options.Source ?? "(none)");

            var computeService = context.GetService<IComputeService>();
            var disk = await computeService.CreateDiskAsync(
                options.Disk!,
                options.ResourceGroup!,
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
                options.DiskAccessId,
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
            _logger.LogError(ex, "Error creating disk. Disk: {Disk}, ResourceGroup: {ResourceGroup}.", options.Disk, options.ResourceGroup);
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
