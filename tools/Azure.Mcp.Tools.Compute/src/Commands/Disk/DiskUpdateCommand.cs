// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
/// Command to update an Azure managed disk.
/// </summary>
public sealed class DiskUpdateCommand(
    ILogger<DiskUpdateCommand> logger)
    : BaseComputeCommand<DiskUpdateOptions>(false)
{
    private const string CommandTitle = "Update Managed Disk";
    private const string CommandDescription =
        "Updates or modifies properties of an existing Azure managed disk that was previously created. "
        + "If resource group is not specified, the disk is located by name within the subscription. "
        + "Supports changing disk size (can only increase), storage SKU, IOPS and throughput limits (UltraSSD only), "
        + "max shares for shared disk attachments, on-demand bursting, tags, "
        + "encryption settings, disk access, and performance tier. "
        + "Modify the network access policy to DenyAll, AllowAll, or AllowPrivate on an existing disk. "
        + "Only specified properties are updated; unspecified properties remain unchanged.";

    private readonly ILogger<DiskUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Id => "4a9b2c3d-6e7f-5b8c-9d0e-1f2a3b4c5d6e";

    public override string Name => "update";

    public override string Title => CommandTitle;

    public override string Description => CommandDescription;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,
        Idempotent = true,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

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
        command.Options.Add(ComputeOptionDefinitions.Tags);
        command.Options.Add(ComputeOptionDefinitions.DiskEncryptionSet);
        command.Options.Add(ComputeOptionDefinitions.EncryptionType);
        command.Options.Add(ComputeOptionDefinitions.DiskAccessId);
        command.Options.Add(ComputeOptionDefinitions.Tier);

        command.Validators.Add(commandResult =>
        {
            if (!commandResult.HasOptionResult(ComputeOptionDefinitions.SizeGb) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Sku) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskIopsReadWrite) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskMbpsReadWrite) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.MaxShares) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.NetworkAccessPolicy) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.EnableBursting) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Tags) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskEncryptionSet) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.EncryptionType) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskAccessId) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Tier))
            {
                commandResult.AddError(
                    "At least one update property must be provided "
                    + "(size-gb, sku, disk-iops-read-write, disk-mbps-read-write, max-shares, "
                    + "network-access-policy, enable-bursting, tags, disk-encryption-set, "
                    + "encryption-type, disk-access-id, or tier).");
            }
        });
    }

    protected override DiskUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Disk = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Disk);
        options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);

        var sizeGb = parseResult.GetValueOrDefault(ComputeOptionDefinitions.SizeGb);
        options.SizeGb = sizeGb > 0 ? sizeGb : null;

        options.Sku = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Sku);

        var iops = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskIopsReadWrite);
        options.DiskIopsReadWrite = iops > 0 ? iops : null;

        var mbps = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskMbpsReadWrite);
        options.DiskMbpsReadWrite = mbps > 0 ? mbps : null;

        var maxShares = parseResult.GetValueOrDefault(ComputeOptionDefinitions.MaxShares);
        options.MaxShares = maxShares > 0 ? maxShares : null;

        options.NetworkAccessPolicy = parseResult.GetValueOrDefault(ComputeOptionDefinitions.NetworkAccessPolicy);
        options.EnableBursting = parseResult.GetValueOrDefault(ComputeOptionDefinitions.EnableBursting);
        options.Tags = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Tags);
        options.DiskEncryptionSet = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskEncryptionSet);
        options.EncryptionType = parseResult.GetValueOrDefault(ComputeOptionDefinitions.EncryptionType);
        options.DiskAccessId = parseResult.GetValueOrDefault(ComputeOptionDefinitions.DiskAccessId);
        options.Tier = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Tier);
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
            var computeService = context.GetService<IComputeService>();

            // If resource group is not provided, search for the disk by name in the subscription
            if (string.IsNullOrEmpty(options.ResourceGroup))
            {
                _logger.LogInformation(
                    "Resource group not specified, searching for disk {DiskName} in subscription {Subscription}",
                    options.Disk, options.Subscription);

                var disks = await computeService.ListDisksAsync(
                    options.Subscription!,
                    null,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var matchingDisks = disks
                    .Where(d => string.Equals(d.Name, options.Disk, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingDisks.Count == 0)
                {
                    throw new ArgumentException($"Disk '{options.Disk}' not found in subscription. Specify --resource-group to narrow the search.");
                }

                if (matchingDisks.Count > 1)
                {
                    var resourceGroups = string.Join(", ", matchingDisks.Select(d => d.ResourceGroup));
                    throw new ArgumentException(
                        $"Multiple disks named '{options.Disk}' found in resource groups: {resourceGroups}. "
                        + "Specify --resource-group to disambiguate.");
                }

                var matchingDisk = matchingDisks[0];
                if (string.IsNullOrEmpty(matchingDisk.ResourceGroup))
                {
                    throw new ArgumentException($"Disk '{options.Disk}' not found in subscription. Specify --resource-group to narrow the search.");
                }

                options.ResourceGroup = matchingDisk.ResourceGroup;
            }

            _logger.LogInformation(
                "Updating disk {DiskName} in resource group {ResourceGroup}",
                options.Disk, options.ResourceGroup);

            var disk = await computeService.UpdateDiskAsync(
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
                options.Tags,
                options.DiskEncryptionSet,
                options.EncryptionType,
                options.DiskAccessId,
                options.Tier,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(disk), ComputeJsonContext.Default.DiskUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating disk. Disk: {Disk}, ResourceGroup: {ResourceGroup}.", options.Disk, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

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
    public record DiskUpdateCommandResult(DiskInfo Disk);
}
