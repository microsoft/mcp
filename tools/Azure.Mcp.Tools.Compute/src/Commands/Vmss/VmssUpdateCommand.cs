// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssUpdateCommand(ILogger<VmssUpdateCommand> logger)
    : BaseComputeCommand<VmssUpdateOptions>(true)
{
    private const string CommandTitle = "Update Virtual Machine Scale Set";
    private readonly ILogger<VmssUpdateCommand> _logger = logger;

    public override string Id => "aaa0ad51-3c16-4ec2-99e2-b24f28a1e7d0";

    public override string Name => "update";

    public override string Description =>
        """
        Update, modify, or reconfigure an existing Azure Virtual Machine Scale Set (VMSS).
        Use this to scale instance count, resize VMs, change upgrade policy, or update tags on a scale set.
        Equivalent to 'az vmss update'. Changes may require 'update-instances' to roll out to existing VMs.
        Do not use this to create a new VMSS (use VMSS create) or to update a single VM (use VM update).
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Required options
        command.Options.Add(ComputeOptionDefinitions.VmssName.AsRequired());

        // Update options (at least one required - validated in command)
        command.Options.Add(ComputeOptionDefinitions.UpgradePolicy);
        command.Options.Add(ComputeOptionDefinitions.Capacity);
        command.Options.Add(ComputeOptionDefinitions.VmSize);
        command.Options.Add(ComputeOptionDefinitions.Overprovision);
        command.Options.Add(ComputeOptionDefinitions.EnableAutoOsUpgrade);
        command.Options.Add(ComputeOptionDefinitions.ScaleInPolicy);
        command.Options.Add(ComputeOptionDefinitions.Tags);

        // Resource group is required for update
        command.Validators.Add(commandResult =>
        {
            // Custom validation: At least one update property must be specified
            if (string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.UpgradePolicy.Name)) &&
                !commandResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.Capacity.Name).HasValue &&
                string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmSize.Name)) &&
                !commandResult.GetValueOrDefault<bool?>(ComputeOptionDefinitions.Overprovision.Name).HasValue &&
                !commandResult.GetValueOrDefault<bool?>(ComputeOptionDefinitions.EnableAutoOsUpgrade.Name).HasValue &&
                string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ScaleInPolicy.Name)) &&
                string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name)))
            {
                commandResult.AddError(
                    "At least one update property must be specified: --upgrade-policy, --capacity, --vm-size, --overprovision, --enable-auto-os-upgrade, --scale-in-policy, or --tags.");
            }
        });
    }

    protected override VmssUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);
        options.UpgradePolicy = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.UpgradePolicy.Name);
        options.Capacity = parseResult.GetValueOrDefault<int?>(ComputeOptionDefinitions.Capacity.Name);
        options.VmSize = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmSize.Name);
        options.Overprovision = parseResult.GetValueOrDefault<bool?>(ComputeOptionDefinitions.Overprovision.Name);
        options.EnableAutoOsUpgrade = parseResult.GetValueOrDefault<bool?>(ComputeOptionDefinitions.EnableAutoOsUpgrade.Name);
        options.ScaleInPolicy = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.ScaleInPolicy.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name);
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

            var result = await computeService.UpdateVmssAsync(
                options.VmssName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VmSize,
                options.Capacity,
                options.UpgradePolicy,
                options.Overprovision,
                options.EnableAutoOsUpgrade,
                options.ScaleInPolicy,
                options.Tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(result), ComputeJsonContext.Default.VmssUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating VMSS. VmssName: {VmssName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.VmssName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "VMSS not found. Verify the VMSS name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to update VMSS. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) =>
            $"Quota exceeded. You may need to request a quota increase for the selected VM size or capacity. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssUpdateCommandResult(VmssUpdateResult Vmss);
}
