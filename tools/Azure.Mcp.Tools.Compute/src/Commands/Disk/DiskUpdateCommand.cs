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
/// Command to update an Azure managed disk.
/// </summary>
public sealed class DiskUpdateCommand(
    ILogger<DiskUpdateCommand> logger,
    IComputeService computeService)
    : BaseComputeCommand<DiskUpdateOptions>
{
    private const string CommandTitle = "Update Managed Disk";
    private const string CommandDescription =
        "Updates properties of an existing Azure managed disk. "
        + "If resource group is not specified, the disk is located by name within the subscription. "
        + "Supports modifying disk size (can only increase), storage SKU, IOPS and throughput limits (UltraSSD only), "
        + "maximum shared attachments, network access policy, and on-demand bursting. "
        + "Only specified properties are updated; unspecified properties remain unchanged.";

    private readonly ILogger<DiskUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    /// <inheritdoc/>
    public override string Id => "4a9b2c3d-6e7f-5b8c-9d0e-1f2a3b4c5d6e";

    /// <inheritdoc/>
    public override string Name => "update";

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override string Description => CommandDescription;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,
        Idempotent = true,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    /// <inheritdoc/>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Disk.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.SizeGb);
        command.Options.Add(ComputeOptionDefinitions.Sku);
        command.Options.Add(ComputeOptionDefinitions.DiskIopsReadWrite);
        command.Options.Add(ComputeOptionDefinitions.DiskMbpsReadWrite);
        command.Options.Add(ComputeOptionDefinitions.MaxShares);
        command.Options.Add(ComputeOptionDefinitions.NetworkAccessPolicy);
        command.Options.Add(ComputeOptionDefinitions.EnableBursting);
    }

    /// <inheritdoc/>
    protected override DiskUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Disk = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Disk.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

        var sizeGb = parseResult.GetValueOrDefault<int>(ComputeOptionDefinitions.SizeGb.Name);
        options.SizeGb = sizeGb > 0 ? sizeGb : null;

        options.Sku = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Sku.Name);

        var iops = parseResult.GetValueOrDefault<long>(ComputeOptionDefinitions.DiskIopsReadWrite.Name);
        options.DiskIopsReadWrite = iops > 0 ? iops : null;

        var mbps = parseResult.GetValueOrDefault<long>(ComputeOptionDefinitions.DiskMbpsReadWrite.Name);
        options.DiskMbpsReadWrite = mbps > 0 ? mbps : null;

        var maxShares = parseResult.GetValueOrDefault<int>(ComputeOptionDefinitions.MaxShares.Name);
        options.MaxShares = maxShares > 0 ? maxShares : null;

        options.NetworkAccessPolicy = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.NetworkAccessPolicy.Name);
        options.EnableBursting = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.EnableBursting.Name);
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
            // If resource group is not provided, search for the disk by name in the subscription
            if (string.IsNullOrEmpty(options.ResourceGroup))
            {
                _logger.LogInformation(
                    "Resource group not specified, searching for disk {DiskName} in subscription {Subscription}",
                    options.Disk, options.Subscription);

                var disks = await _computeService.ListDisksAsync(
                    options.Subscription!,
                    null,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var matchingDisk = disks.FirstOrDefault(d =>
                    string.Equals(d.Name, options.Disk, StringComparison.OrdinalIgnoreCase));

                if (matchingDisk == null || string.IsNullOrEmpty(matchingDisk.ResourceGroup))
                {
                    throw new ArgumentException($"Disk '{options.Disk}' not found in subscription. Specify --resource-group to narrow the search.");
                }

                options.ResourceGroup = matchingDisk.ResourceGroup;
            }

            _logger.LogInformation(
                "Updating disk {DiskName} in resource group {ResourceGroup}",
                options.Disk, options.ResourceGroup);

            var disk = await _computeService.UpdateDiskAsync(
                options.Disk!,
                options.ResourceGroup!,
                options.Subscription!,
                options.SizeGb,
                options.Sku,
                options.DiskIopsReadWrite,
                options.DiskMbpsReadWrite,
                options.MaxShares,
                options.NetworkAccessPolicy,
                options.EnableBursting,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DiskUpdateCommandResult(disk),
                ComputeJsonContext.Default.DiskUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating disk. Options: {@Options}", options);
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
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Disk not found. Verify the disk name and resource group are correct.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx when reqEx.Status == 409 =>
            $"Conflict updating disk. The disk may be in use or the requested change is not allowed. Details: {reqEx.Message}",
        Azure.Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx =>
            $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the disk update command.
    /// </summary>
    public record DiskUpdateCommandResult(DiskInfo Disk);
}
