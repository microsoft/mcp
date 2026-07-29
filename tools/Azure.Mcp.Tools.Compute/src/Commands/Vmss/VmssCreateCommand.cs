// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Azure.Mcp.Tools.Compute.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

[CommandMetadata(
    Id = "c46a4bc5-cba6-4d99-991b-a9109fc689ad",
    Name = "create",
    Title = "Create Virtual Machine Scale Set",
    Description = """
        Create, deploy, or provision a new Azure Virtual Machine Scale Set (VMSS) for running multiple identical VM instances.
        Use this to deploy a brand new VMSS that needs horizontal scaling, load balancing, or high availability across instances,
        including specifying the initial instance count (e.g., 3 instances, 5 instances) and upgrade policy
        (Manual, Automatic, or Rolling) at creation time.
        Equivalent to 'az vmss create'. Defaults to 2 instances and Standard_D2s_v5 size when not specified.
        The --image option is required and has no default; if the user does not specify an image, ask them which image to use
        (an alias such as 'Ubuntu2404' or 'Win2022Datacenter', a marketplace URN like 'publisher:offer:sku:version',
        or a shared gallery image ID starting with '/sharedGalleries/').
        For Linux VMSS with SSH, read the user's public key file (e.g., ~/.ssh/id_rsa.pub) and pass its content.
        Do not use this for creating a single standalone VM (use VM create instead).
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = true,
    LocalRequired = false)]
public sealed class VmssCreateCommand(ILogger<VmssCreateCommand> logger, IComputeService computeService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VmssCreateOptions, VmssCreateCommand.VmssCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VmssCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    public override void ValidateOptions(VmssCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Determine OS type from image
        var effectiveOsType = ComputeUtilities.DetermineOsType(options.OsType, options.Image);

        // Custom validation: For Windows VMSS, password is required
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(options.AdminPassword))
        {
            validationResult.Errors.Add("The --admin-password option is required for Windows VMSS.");
        }

        // Custom validation: For Windows VMSS, name cannot exceed 9 characters (Azure adds 6-char suffix for computer name)
        if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && options.VmssName?.Length > 9)
        {
            validationResult.Errors.Add(
                "Windows VMSS name cannot exceed 9 characters. Azure appends a 6-character suffix to create the computer name, " +
                "and Windows computer names are limited to 15 characters total.");
        }

        // Custom validation: For Linux VMSS, either SSH key or password must be provided
        if (effectiveOsType.Equals("linux", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrEmpty(options.SshPublicKey) &&
            string.IsNullOrEmpty(options.AdminPassword))
        {
            validationResult.Errors.Add(
                "Linux VMSS require authentication. Please provide either --ssh-public-key or --admin-password. " +
                "To use SSH, first read the user's public key file (e.g., ~/.ssh/id_rsa.pub or ~/.ssh/id_ed25519.pub) " +
                "and pass the full key content to --ssh-public-key.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VmssCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await _computeService.CreateVmssAsync(
                options.VmssName,
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
                options.InstanceCount,
                options.UpgradePolicy,
                options.Zone,
                options.OsDiskSizeGb,
                options.OsDiskType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(result), ComputeJsonContext.Default.VmssCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating VMSS. VmssName: {VmssName}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}",
                options.VmssName, options.ResourceGroup, options.Location, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource not found. Verify the resource group exists and you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to create VMSS. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"A VMSS with the specified name already exists. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) =>
            $"Quota exceeded. You may need to request a quota increase for the selected VM size. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VmssCreateCommandResult(VmssCreateResult Vmss);
}
