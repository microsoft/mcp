// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Azure.Mcp.Tools.Compute.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmCreateCommand(ILogger<VmCreateCommand> logger)
    : BaseComputeCommand<VmCreateOptions>()
{
    private const string CommandTitle = "Create Virtual Machine";
    private readonly ILogger<VmCreateCommand> _logger = logger;

    public override string Id => "d4c9b2e7-5f3a-4b8e-9c1d-0e2f3a4b5c6d";

    public override string Name => "create";

    public override string Description =>
        """
        Create an Azure Virtual Machine with smart defaults based on workload requirements.
        Supports automatic VM size selection based on workload type (development, web, database, compute, memory, gpu, general).
        Creates necessary network resources (VNet, subnet, NSG, NIC, public IP) if not specified.
        Supports both Linux and Windows VMs with SSH key or password authentication.

        Workload types and suggested configurations:
        - development: Standard_B2s - Cost-effective burstable VM for dev/test
        - web: Standard_D2s_v3 - General purpose for web servers
        - database: Standard_E4s_v3 - Memory-optimized for databases
        - compute: Standard_F4s_v2 - CPU-optimized for batch processing
        - memory: Standard_E8s_v3 - High-memory for caching
        - gpu: Standard_NC6s_v3 - GPU-enabled for ML/rendering
        - general: Standard_D2s_v3 - Balanced general purpose

        Required options:
        - --vm-name: Name of the VM to create
        - --resource-group: Resource group name
        - --subscription: Subscription ID or name
        - --location: Azure region
        - --admin-username: Admin username

        Authentication requirements:
        - For Windows VMs: --admin-password is required
        - For Linux VMs: Either --ssh-public-key OR --admin-password is required

        IMPORTANT for Linux VMs with SSH authentication:
        Before calling this tool, you must first read the user's SSH public key file (typically ~/.ssh/id_rsa.pub,
        ~/.ssh/id_ed25519.pub, or similar) and pass the full key content to --ssh-public-key.
        The SSH public key is safe to share - it contains no secrets.
        Example: --ssh-public-key "ssh-ed25519 AAAAC3... user@host"
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
        command.Options.Add(ComputeOptionDefinitions.Workload);
        command.Options.Add(ComputeOptionDefinitions.OsType);

        // Network options
        command.Options.Add(ComputeOptionDefinitions.VirtualNetwork);
        command.Options.Add(ComputeOptionDefinitions.Subnet);
        command.Options.Add(ComputeOptionDefinitions.PublicIpAddress);
        command.Options.Add(ComputeOptionDefinitions.NetworkSecurityGroup);
        command.Options.Add(ComputeOptionDefinitions.NoPublicIp);

        // Additional options
        command.Options.Add(ComputeOptionDefinitions.Zone);
        command.Options.Add(ComputeOptionDefinitions.OsDiskSizeGb);
        command.Options.Add(ComputeOptionDefinitions.OsDiskType);

        // Resource group is required for create
        command.Validators.Add(commandResult =>
        {
            var resourceGroup = commandResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
            if (string.IsNullOrEmpty(resourceGroup))
            {
                commandResult.AddError($"Missing Required option: {OptionDefinitions.Common.ResourceGroup.Name}");
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
        options.Workload = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Workload.Name);
        options.OsType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsType.Name);
        options.VirtualNetwork = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VirtualNetwork.Name);
        options.Subnet = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Subnet.Name);
        options.PublicIpAddress = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.PublicIpAddress.Name);
        options.NetworkSecurityGroup = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.NetworkSecurityGroup.Name);
        options.NoPublicIp = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.NoPublicIp.Name);
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

        // Determine OS type from image
        var effectiveOsType = ComputeUtilities.DetermineOsType(options.OsType, options.Image);

        // Custom validation: For Windows VMs, password is required
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(options.AdminPassword))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The --admin-password option is required for Windows VMs.";
            return context.Response;
        }

        // Custom validation: For Windows VMs, computer name cannot exceed 15 characters
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && options.VmName!.Length > 15)
        {
            throw new CommandValidationException(
                VmRequirements.WindowsComputerName,
                HttpStatusCode.BadRequest);
        }

        // Custom validation: For Linux VMs, either SSH key or password must be provided
        if (effectiveOsType.Equals("linux", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrEmpty(options.SshPublicKey) &&
            string.IsNullOrEmpty(options.AdminPassword))
        {
            throw new CommandValidationException(
                "Linux VMs require authentication. Please provide either --ssh-public-key or --admin-password. " +
                "To use SSH, first read the user's public key file (e.g., ~/.ssh/id_rsa.pub or ~/.ssh/id_ed25519.pub) " +
                "and pass the full key content to --ssh-public-key.",
                HttpStatusCode.BadRequest);
        }

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
                options.Workload,
                options.OsType,
                options.VirtualNetwork,
                options.Subnet,
                options.PublicIpAddress,
                options.NetworkSecurityGroup,
                options.NoPublicIp,
                options.Zone,
                options.OsDiskSizeGb,
                options.OsDiskType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VmCreateCommandResult(result),
                ComputeJsonContext.Default.VmCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating VM. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}, Workload: {Workload}",
                options.VmName, options.ResourceGroup, options.Location, options.Subscription, options.Workload);
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
