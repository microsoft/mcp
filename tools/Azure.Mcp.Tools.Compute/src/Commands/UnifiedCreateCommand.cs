// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Services;
using Azure.Mcp.Tools.Compute.Utilities;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands;

[CommandMetadata(
    Id = "8d1c2f3e-9a4b-4c5d-8e6f-1a2b3c4d5e6f",
    Name = "create",
    Title = "Create Compute (VMSS Flex by default, single VM with --single-instance)",
    Description = """
        Recommended top-level entry point for creating Azure compute. Creates a VMSS Flex scale set by default,
        which works equally well for 1 instance or N and is the GA orchestration mode for both fixed-size and
        elastic workloads. Pass --single-instance only when the workload can never scale out, never needs zonal
        spread, and never needs rolling upgrades — in that case this command falls back to creating a single
        non-scalable VM (equivalent to 'compute vm create').
        Defaults: 2 instances on the VMSS Flex path, Standard_D2s_v5 VM size, Manual upgrade policy.
        The --image option is required and has no default; if the user does not specify an image, ask which image
        to use (an alias such as 'Ubuntu2404' or 'Win2022Datacenter', a marketplace URN like
        'publisher:offer:sku:version', or a shared gallery image ID starting with '/sharedGalleries/').
        For Linux with SSH, read the user's public key file (e.g., ~/.ssh/id_rsa.pub) and pass its content.
        The --instance-count and --upgrade-policy options are ignored when --single-instance is set.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class UnifiedCreateCommand(ILogger<UnifiedCreateCommand> logger, IComputeService computeService)
    : BaseComputeCommand<UnifiedCreateOptions>(true)
{
    private readonly ILogger<UnifiedCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Required options — VmssName carries the canonical "--name" alias and is reused for both dispatch paths.
        command.Options.Add(ComputeOptionDefinitions.VmssName.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.AdminUsername.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Image.AsRequired());

        // Authentication options (at least one required - validated below)
        command.Options.Add(ComputeOptionDefinitions.AdminPassword);
        command.Options.Add(ComputeOptionDefinitions.SshPublicKey);

        // Optional configuration
        command.Options.Add(ComputeOptionDefinitions.VmSize);
        command.Options.Add(ComputeOptionDefinitions.OsType);

        // VMSS-only options (ignored when --single-instance is set)
        command.Options.Add(ComputeOptionDefinitions.InstanceCount);
        command.Options.Add(ComputeOptionDefinitions.UpgradePolicy);

        // Network options (shared)
        command.Options.Add(ComputeOptionDefinitions.VirtualNetwork);
        command.Options.Add(ComputeOptionDefinitions.Subnet);
        command.Options.Add(ComputeOptionDefinitions.PublicIpAddress);
        command.Options.Add(ComputeOptionDefinitions.NetworkSecurityGroup);
        command.Options.Add(ComputeOptionDefinitions.NoPublicIp);
        command.Options.Add(ComputeOptionDefinitions.SourceAddressPrefix);

        // Additional options
        command.Options.Add(ComputeOptionDefinitions.Zone);
        command.Options.Add(ComputeOptionDefinitions.OsDiskSizeGb);
        command.Options.Add(ComputeOptionDefinitions.OsDiskType);

        // Dispatch toggle: default false => VMSS Flex; true => single non-scalable VM.
        command.Options.Add(ComputeOptionDefinitions.SingleInstance);

        command.Validators.Add(commandResult =>
        {
            var effectiveOsType = ComputeUtilities.DetermineOsType(
                commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsType.Name),
                commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Image.Name));

            var adminPassword = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.AdminPassword.Name);
            var singleInstance = commandResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.SingleInstance.Name);
            var name = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);

            // Windows requires a password for either dispatch path.
            if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(adminPassword))
            {
                commandResult.AddError("The --admin-password option is required for Windows compute.");
            }

            // Windows name length differs by dispatch path:
            // - Single VM: 15 chars (Windows computer name limit).
            // - VMSS Flex: 9 chars (Azure appends a 6-char suffix to derive each instance's computer name).
            if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && name is not null)
            {
                if (singleInstance && name.Length > 15)
                {
                    commandResult.AddError(VmRequirements.WindowsComputerName);
                }
                else if (!singleInstance && name.Length > 9)
                {
                    commandResult.AddError(
                        "Windows VMSS name cannot exceed 9 characters. Azure appends a 6-character suffix to create the computer name, " +
                        "and Windows computer names are limited to 15 characters total. " +
                        "Either shorten the name or pass --single-instance to fall back to a standalone VM (15-char limit).");
                }
            }

            // Linux requires either SSH key or password (same on both paths).
            if (effectiveOsType.Equals("linux", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SshPublicKey.Name)) &&
                string.IsNullOrEmpty(adminPassword))
            {
                commandResult.AddError(
                    "Linux compute requires authentication. Please provide either --ssh-public-key or --admin-password. " +
                    "To use SSH, first read the user's public key file (e.g., ~/.ssh/id_rsa.pub or ~/.ssh/id_ed25519.pub) " +
                    "and pass the full key content to --ssh-public-key.");
            }
        });
    }

    protected override UnifiedCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.AdminUsername = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.AdminUsername.Name);
        options.AdminPassword = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.AdminPassword.Name);
        options.SshPublicKey = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SshPublicKey.Name);
        options.VmSize = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmSize.Name);
        options.Image = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Image.Name);
        options.OsType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsType.Name);
        options.InstanceCount = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.InstanceCount.Name);
        options.UpgradePolicy = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.UpgradePolicy.Name);
        options.VirtualNetwork = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VirtualNetwork.Name);
        options.Subnet = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Subnet.Name);
        options.PublicIpAddress = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.PublicIpAddress.Name);
        options.NetworkSecurityGroup = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.NetworkSecurityGroup.Name);
        options.NoPublicIp = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.NoPublicIp.Name);
        options.SourceAddressPrefix = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SourceAddressPrefix.Name);
        options.Zone = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Zone.Name);
        options.OsDiskSizeGb = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.OsDiskSizeGb.Name);
        options.OsDiskType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsDiskType.Name);
        options.SingleInstance = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.SingleInstance.Name);
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
            context.Activity?.AddTag("subscription", options.Subscription);
            context.Activity?.AddTag("singleInstance", options.SingleInstance);

            if (options.SingleInstance)
            {
                var vmResult = await _computeService.CreateVmAsync(
                    options.Name!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Location!,
                    options.AdminUsername!,
                    options.VmSize,
                    options.Image,
                    options.AdminPassword,
                    options.SshPublicKey,
                    options.OsType,
                    options.VirtualNetwork,
                    options.Subnet,
                    options.PublicIpAddress,
                    options.NetworkSecurityGroup,
                    options.NoPublicIp,
                    options.SourceAddressPrefix,
                    options.Zone,
                    options.OsDiskSizeGb,
                    options.OsDiskType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new UnifiedCreateCommandResult(Vmss: null, Vm: vmResult, Mode: "single-vm"),
                    ComputeJsonContext.Default.UnifiedCreateCommandResult);
            }
            else
            {
                var vmssResult = await _computeService.CreateVmssAsync(
                    options.Name!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Location!,
                    options.AdminUsername!,
                    options.VmSize,
                    options.Image,
                    options.AdminPassword,
                    options.SshPublicKey,
                    options.OsType,
                    options.VirtualNetwork,
                    options.Subnet,
                    options.PublicIpAddress,
                    options.NetworkSecurityGroup,
                    options.NoPublicIp,
                    options.SourceAddressPrefix,
                    options.InstanceCount,
                    options.UpgradePolicy,
                    options.Zone,
                    options.OsDiskSizeGb,
                    options.OsDiskType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new UnifiedCreateCommandResult(Vmss: vmssResult, Vm: null, Mode: "vmss-flex"),
                    ComputeJsonContext.Default.UnifiedCreateCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating compute. Name: {Name}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}, SingleInstance: {SingleInstance}",
                options.Name, options.ResourceGroup, options.Location, options.Subscription, options.SingleInstance);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource not found. Verify the resource group exists and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to create compute. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"A compute resource with the specified name already exists. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) =>
            $"Quota exceeded. You may need to request a quota increase for the selected VM size. Try `compute check-quota --location <region>` first. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Discriminated result for the unified create. Exactly one of <see cref="Vmss"/> or <see cref="Vm"/> is populated;
    /// <see cref="Mode"/> ("vmss-flex" or "single-vm") tells the caller which path ran.
    /// </summary>
    internal record UnifiedCreateCommandResult(VmssCreateResult? Vmss, VmCreateResult? Vm, string Mode);
}
