// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Azure.Mcp.Tools.Compute.Utilities;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmCreateCommand(ILogger<VmCreateCommand> logger)
    : BaseComputeCommand<VmCreateOptions>(true)
{
    private const string CommandTitle = "Create Virtual Machine";
    private readonly ILogger<VmCreateCommand> _logger = logger;

    public override string Id => "b765ab9c-788d-4422-80aa-54488f6be648";

    public override string Name => "create";

    public override string Description =>
        """
        Create, deploy, or provision a single Azure Virtual Machine (VM).
        Use this to launch a new Linux or Windows VM with SSH key or password authentication.
        Automatically creates networking resources (VNet, subnet, NSG, NIC, public IP) when not specified.
        Equivalent to 'az vm create'. Defaults to Standard_DS1_v2 size and Ubuntu 24.04 LTS if not specified.
        For Linux VMs with SSH, read the user's public key file (e.g., ~/.ssh/id_rsa.pub) and pass its content.
        Do not use this for creating Virtual Machine Scale Sets with multiple identical instances (use VMSS create instead).
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Required options
        command.Options.Add(ComputeOptionDefinitions.VmName.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.AdminUsername.AsRequired());

        // Authentication options (at least one required - validated in command)
        command.Options.Add(ComputeOptionDefinitions.AdminPassword);
        command.Options.Add(ComputeOptionDefinitions.SshPublicKey);

        // Optional configuration
        command.Options.Add(ComputeOptionDefinitions.VmSize);
        command.Options.Add(ComputeOptionDefinitions.Image);
        command.Options.Add(ComputeOptionDefinitions.OsType);

        // Network options
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

        // Resource group is required for create
        command.Validators.Add(commandResult =>
        {
            var adminPassword = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.AdminPassword.Name);
            // Determine OS type from image
            var osType = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsType.Name);
            var image = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Image.Name);
            var effectiveOsType = ComputeUtilities.DetermineOsType(osType, image);

            // Custom validation: For Windows VMs, password is required
            if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(adminPassword))
            {
                commandResult.AddError("The --admin-password option is required for Windows VMs.");
            }

            // Custom validation: For Windows VMs, computer name cannot exceed 15 characters
            if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase)
                && commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name)?.Length > 15)
            {
                commandResult.AddError(VmRequirements.WindowsComputerName);
            }

            // Custom validation: For Linux VMs, either SSH key or password must be provided
            if (effectiveOsType.Equals("linux", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SshPublicKey.Name)) &&
                string.IsNullOrEmpty(adminPassword))
            {
                commandResult.AddError(
                    "Linux VMs require authentication. Please provide either --ssh-public-key or --admin-password. " +
                    "To use SSH, first read the user's public key file (e.g., ~/.ssh/id_rsa.pub or ~/.ssh/id_ed25519.pub) " +
                    "and pass the full key content to --ssh-public-key.");
            }
        });
    }

    protected override VmCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
        options.Location = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Location.Name);
        options.AdminUsername = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.AdminUsername.Name);
        options.AdminPassword = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.AdminPassword.Name);
        options.SshPublicKey = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SshPublicKey.Name);
        options.VmSize = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmSize.Name);
        options.Image = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Image.Name);
        options.OsType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsType.Name);
        options.VirtualNetwork = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VirtualNetwork.Name);
        options.Subnet = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Subnet.Name);
        options.PublicIpAddress = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.PublicIpAddress.Name);
        options.NetworkSecurityGroup = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.NetworkSecurityGroup.Name);
        options.NoPublicIp = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.NoPublicIp.Name);
        options.SourceAddressPrefix = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SourceAddressPrefix.Name);
        options.Zone = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Zone.Name);
        options.OsDiskSizeGb = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.OsDiskSizeGb.Name);
        options.OsDiskType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsDiskType.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        var computeService = context.GetService<IComputeService>();

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await computeService.CreateVmAsync(
                options.VmName!,
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

            context.Response.Results = ResponseResult.Create(new(result), ComputeJsonContext.Default.VmCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating VM. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}",
                options.VmName, options.ResourceGroup, options.Location, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource not found. Verify the resource group exists and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to create VMs. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"A VM with the specified name already exists. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) =>
            $"Quota exceeded. You may need to request a quota increase for the selected VM size. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmCreateCommandResult(VmCreateResult Vm);
}
