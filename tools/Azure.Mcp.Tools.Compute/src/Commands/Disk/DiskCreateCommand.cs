// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Disk;

/// <summary>
/// Command to create an Azure managed disk.
/// </summary>
public sealed class DiskCreateCommand(
    ILogger<DiskCreateCommand> logger,
    IComputeService computeService)
    : BaseComputeCommand<DiskCreateOptions>
{
    private const string CommandTitle = "Create Managed Disk";
    private const string CommandDescription =
        "Creates a new Azure managed disk in the specified resource group. "
        + "Supports creating empty disks (specify --size-gb) or disks from a source such as a snapshot, "
        + "another managed disk, or a blob URI (specify --source). "
        + "If location is not specified, defaults to the resource group's location. "
        + "Supports configuring disk size, storage SKU (e.g., Premium_LRS, Standard_LRS, UltraSSD_LRS), "
        + "OS type, availability zone, hypervisor generation, tags, encryption settings, "
        + "performance tier, shared disk, network access, and on-demand bursting.";

    private readonly ILogger<DiskCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    /// <inheritdoc/>
    public override string Id => "3f8a1b2c-5d6e-4a7b-8c9d-0e1f2a3b4c5d";

    /// <inheritdoc/>
    public override string Name => "create";

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override string Description => CommandDescription;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,
        Idempotent = false,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    /// <inheritdoc/>
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
    }

    /// <inheritdoc/>
    protected override DiskCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Disk = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Disk.Name);
        options.Source = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Source.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

        var sizeGb = parseResult.GetValueOrDefault<int>(ComputeOptionDefinitions.SizeGb.Name);
        options.SizeGb = sizeGb > 0 ? sizeGb : null;

        options.Sku = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Sku.Name);
        options.OsType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsType.Name);
        options.Zone = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Zone.Name);
        options.HyperVGeneration = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.HyperVGeneration.Name);

        var maxShares = parseResult.GetValueOrDefault<int>(ComputeOptionDefinitions.MaxShares.Name);
        options.MaxShares = maxShares > 0 ? maxShares : null;

        options.NetworkAccessPolicy = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.NetworkAccessPolicy.Name);
        options.EnableBursting = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.EnableBursting.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name);
        options.DiskEncryptionSet = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.DiskEncryptionSet.Name);
        options.EncryptionType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.EncryptionType.Name);
        options.DiskAccessId = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.DiskAccessId.Name);
        options.Tier = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tier.Name);
        return options;
    }

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            if (string.IsNullOrEmpty(options.ResourceGroup))
            {
                throw new ArgumentException("Resource group is required for creating a disk.");
            }

            if (string.IsNullOrEmpty(options.Source) && !options.SizeGb.HasValue)
            {
                throw new ArgumentException("Either --source or --size-gb must be specified.");
            }

            _logger.LogInformation(
                "Creating disk {DiskName} in resource group {ResourceGroup}, location {Location}, source {Source}",
                options.Disk, options.ResourceGroup, options.Location ?? "(default)", options.Source ?? "(none)");

            var disk = await _computeService.CreateDiskAsync(
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
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DiskCreateCommandResult(disk),
                ComputeJsonContext.Default.DiskCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating disk. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <inheritdoc/>
    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Azure.Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    /// <inheritdoc/>
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 409 =>
            "A disk with this name already exists in the resource group.",
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Resource group not found. Verify the resource group name is correct.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed. Details: {reqEx.Message}",
        Azure.Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx =>
            $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the disk create command.
    /// </summary>
    public record DiskCreateCommandResult(DiskInfo Disk);
}
