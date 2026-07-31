// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Azure.Mcp.Tools.Compute.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

[CommandMetadata(
    Id = "b765ab9c-788d-4422-80aa-54488f6be648",
    Name = "create",
    Title = "Create Virtual Machine",
    Description = """
        Create, deploy, or provision a single Azure Virtual Machine (VM) with its OS disk.
        Use this to launch a new Linux or Windows VM with SSH key or password authentication.
        Automatically creates networking resources (VNet, subnet, NSG, NIC, public IP) when not specified.
        Equivalent to 'az vm create'. Defaults to Standard_D2s_v5 VM size when not specified.
        The --image option is required and has no default; if the user does not specify an image, ask them which image to use
        (an alias such as 'Ubuntu2404' or 'Win2022Datacenter', a marketplace URN like 'publisher:offer:sku:version',
        or a shared gallery image ID starting with '/sharedGalleries/').
        For Linux VMs with SSH, read the user's public key file (e.g., ~/.ssh/id_rsa.pub) and pass its content.
        Do not use this for Virtual Machine Scale Sets with multiple identical instances (use VMSS create instead).
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class VmCreateCommand(ILogger<VmCreateCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmCreateOptions, VmCreateCommand.VmCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VmCreateCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override void ValidateOptions(VmCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Determine OS type from image
        var effectiveOsType = ComputeUtilities.DetermineOsType(options.OsType, options.Image);

        // Custom validation: For Windows VMs, password is required
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrEmpty(options.AdminPassword))
        {
            validationResult.Errors.Add("The --admin-password option is required for Windows VMs.");
        }

        // Custom validation: For Windows VMs, computer name cannot exceed 15 characters
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && options.VmName?.Length > 15)
        {
            validationResult.Errors.Add(VmRequirements.WindowsComputerName);
        }

        // Custom validation: For Linux VMs, either SSH key or password must be provided
        if (effectiveOsType.Equals("linux", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrEmpty(options.SshPublicKey) &&
            string.IsNullOrEmpty(options.AdminPassword))
        {
            validationResult.Errors.Add(
                "Linux VMs require authentication. Please provide either --ssh-public-key or --admin-password. " +
                "To use SSH, first read the user's public key file (e.g., ~/.ssh/id_rsa.pub or ~/.ssh/id_ed25519.pub) " +
                "and pass the full key content to --ssh-public-key.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VmCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await _computeService.CreateVmAsync(
                options.VmName,
                options.ResourceGroup,
                options.Subscription!,
                options.Location,
                options.AdminUsername,
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

    public sealed record VmCreateCommandResult(VmCreateResult Vm);
}
