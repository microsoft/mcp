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

[CommandMetadata(
    Id = "aaa0ad51-3c16-4ec2-99e2-b24f28a1e7d0",
    Name = "update",
    Title = "Update Virtual Machine Scale Set",
    Description = """
        Update, modify, or reconfigure an existing Azure Virtual Machine Scale Set (VMSS).
        Use this to scale instance count, resize VMs, change upgrade policy, or update tags on a scale set.
        Equivalent to 'az vmss update'. Changes may require 'update-instances' to roll out to existing VMs.
        Do not use this to create a new VMSS (use VMSS create) or to update a single VM (use VM update).
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VmssUpdateCommand(ILogger<VmssUpdateCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmssUpdateOptions>(true)
{
    private readonly ILogger<VmssUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

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
            if (!commandResult.HasOptionResult(ComputeOptionDefinitions.UpgradePolicy) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Capacity) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.VmSize) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Overprovision) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.EnableAutoOsUpgrade) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.ScaleInPolicy) &&
                !commandResult.HasOptionResult(ComputeOptionDefinitions.Tags))
            {
                commandResult.AddError(
                    "At least one update property must be specified: --upgrade-policy, --capacity, --vm-size, --overprovision, --enable-auto-os-upgrade, --scale-in-policy, or --tags.");
            }
        });
    }

    protected override VmssUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault(ComputeOptionDefinitions.VmssName);
        options.UpgradePolicy = parseResult.GetValueOrDefault(ComputeOptionDefinitions.UpgradePolicy);
        options.Capacity = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Capacity);
        options.VmSize = parseResult.GetValueOrDefault(ComputeOptionDefinitions.VmSize);
        options.Overprovision = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Overprovision);
        options.EnableAutoOsUpgrade = parseResult.GetValueOrDefault(ComputeOptionDefinitions.EnableAutoOsUpgrade);
        options.ScaleInPolicy = parseResult.GetValueOrDefault(ComputeOptionDefinitions.ScaleInPolicy);
        options.Tags = parseResult.GetValueOrDefault(ComputeOptionDefinitions.Tags);
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

            var result = await _computeService.UpdateVmssAsync(
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
