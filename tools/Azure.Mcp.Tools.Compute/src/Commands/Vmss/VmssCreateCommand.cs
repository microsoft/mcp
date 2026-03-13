// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Azure.Mcp.Tools.Compute.Utilities;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssCreateCommand(ILogger<VmssCreateCommand> logger)
    : BaseComputeCommand<VmssCreateOptions>(true)
{
    private const string CommandTitle = "Create Virtual Machine Scale Set";
    private readonly ILogger<VmssCreateCommand> _logger = logger;

    public override string Id => "c46a4bc5-cba6-4d99-991b-a9109fc689ad";

    public override string Name => "create";

    public override string Description =>
        """
        Create, deploy, or provision an Azure Virtual Machine Scale Set (VMSS) for running multiple identical VM instances.
        Use this to deploy workloads that need horizontal scaling, load balancing, or high availability across instances.
        Equivalent to 'az vmss create'. Defaults to 2 instances, Standard_DS1_v2 size, and Ubuntu 24.04 LTS.
        For Linux VMSS with SSH, read the user's public key file (e.g., ~/.ssh/id_rsa.pub) and pass its content.
        Do not use this for creating a single standalone VM (use VM create instead).
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
        command.Options.Add(ComputeOptionDefinitions.VmssName.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.Location.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.AdminUsername.AsRequired());

        // Authentication options (at least one required - validated in command)
        command.Options.Add(ComputeOptionDefinitions.AdminPassword);
        command.Options.Add(ComputeOptionDefinitions.SshPublicKey);

        // Optional configuration
        command.Options.Add(ComputeOptionDefinitions.VmSize);
        command.Options.Add(ComputeOptionDefinitions.Image);
        command.Options.Add(ComputeOptionDefinitions.OsType);

        // VMSS-specific options
        command.Options.Add(ComputeOptionDefinitions.InstanceCount);
        command.Options.Add(ComputeOptionDefinitions.UpgradePolicy);

        // Network options
        command.Options.Add(ComputeOptionDefinitions.VirtualNetwork);
        command.Options.Add(ComputeOptionDefinitions.Subnet);

        // Additional options
        command.Options.Add(ComputeOptionDefinitions.Zone);
        command.Options.Add(ComputeOptionDefinitions.OsDiskSizeGb);
        command.Options.Add(ComputeOptionDefinitions.OsDiskType);

        // Resource group is required for create
        command.Validators.Add(commandResult =>
        {
            // Determine OS type from image
            var effectiveOsType = ComputeUtilities.DetermineOsType(
                commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.OsType.Name),
                commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Image.Name));

            var adminPassword = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.AdminPassword.Name);
            // Custom validation: For Windows VMSS, password is required
            if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(adminPassword))
            {
                commandResult.AddError("The --admin-password option is required for Windows VMSS.");
            }

            // Custom validation: For Windows VMSS, name cannot exceed 9 characters (Azure adds 6-char suffix for computer name)
            if (effectiveOsType.Equals("windows", StringComparison.OrdinalIgnoreCase)
                && commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name)?.Length > 9)
            {
                commandResult.AddError(
                    "Windows VMSS name cannot exceed 9 characters. Azure appends a 6-character suffix to create the computer name, " +
                    "and Windows computer names are limited to 15 characters total.");
            }

            // Custom validation: For Linux VMSS, either SSH key or password must be provided
            if (effectiveOsType.Equals("linux", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.SshPublicKey.Name)) &&
                string.IsNullOrEmpty(adminPassword))
            {
                commandResult.AddError(
                    "Linux VMSS require authentication. Please provide either --ssh-public-key or --admin-password. " +
                    "To use SSH, first read the user's public key file (e.g., ~/.ssh/id_rsa.pub or ~/.ssh/id_ed25519.pub) " +
                    "and pass the full key content to --ssh-public-key.");
            }
        });
    }

    protected override VmssCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);
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

            var result = await computeService.CreateVmssAsync(
                options.VmssName!,
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

    internal record VmssCreateCommandResult(VmssCreateResult Vmss);
}
