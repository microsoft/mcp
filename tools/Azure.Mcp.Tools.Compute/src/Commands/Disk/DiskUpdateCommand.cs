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
            if (!commandResult.HasOptionResult(ComputeOptionDefinitions.SizeGb.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Sku.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskIopsReadWrite.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskMbpsReadWrite.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.MaxShares.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.NetworkAccessPolicy.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.EnableBursting.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Tags.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskEncryptionSet.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.EncryptionType.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.DiskAccessId.Name) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Tier.Name))
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
        options.Tags = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name);
        options.DiskEncryptionSet = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.DiskEncryptionSet.Name);
        options.EncryptionType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.EncryptionType.Name);
        options.DiskAccessId = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.DiskAccessId.Name);
        options.Tier = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tier.Name);
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
