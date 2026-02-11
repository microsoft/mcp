// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssUpdateCommand(ILogger<VmssUpdateCommand> logger)
    : BaseComputeCommand<VmssUpdateOptions>()
{
    private const string CommandTitle = "Update Virtual Machine Scale Set";
    private readonly ILogger<VmssUpdateCommand> _logger = logger;

    public override string Id => "f6e1d4g9-7h5c-6d0g-1e3f-2g4h5i6j7k8l";

    public override string Name => "update";

    public override string Description =>
        """
        Update an existing Azure Virtual Machine Scale Set (VMSS) configuration.
        Supports updating upgrade policy, capacity (instance count), VM size, and other properties.
        Uses PATCH semantics - only specified properties are updated.

        Updatable properties:
        - --upgrade-policy: Change upgrade policy mode (Automatic, Manual, Rolling)
        - --capacity: Change the number of VM instances
        - --vm-size: Change the VM SKU size
        - --overprovision: Enable or disable overprovisioning
        - --enable-auto-os-upgrade: Enable or disable automatic OS image upgrades
        - --scale-in-policy: Set scale-in policy (Default, OldestVM, NewestVM)
        - --tags: Add or update tags in key=value,key2=value2 format

        Required options:
        - --vmss-name: Name of the VMSS to update
        - --resource-group: Resource group name
        - --subscription: Subscription ID or name

        At least one update property must be specified.

        Examples:
        - Update upgrade policy: --upgrade-policy Automatic
        - Scale to 5 instances: --capacity 5
        - Change VM size: --vm-size Standard_D4s_v3
        - Enable auto OS upgrade: --enable-auto-os-upgrade true
        - Set scale-in policy: --scale-in-policy OldestVM
        - Add tags: --tags environment=prod,team=compute
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
            var resourceGroup = commandResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);
            if (string.IsNullOrEmpty(resourceGroup))
            {
                commandResult.AddError($"Missing Required option: {OptionDefinitions.Common.ResourceGroup.Name}");
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

        // Custom validation: At least one update property must be specified
        if (string.IsNullOrEmpty(options.UpgradePolicy) &&
            !options.Capacity.HasValue &&
            string.IsNullOrEmpty(options.VmSize) &&
            !options.Overprovision.HasValue &&
            !options.EnableAutoOsUpgrade.HasValue &&
            string.IsNullOrEmpty(options.ScaleInPolicy) &&
            string.IsNullOrEmpty(options.Tags))
        {
            throw new CommandValidationException(
                "At least one update property must be specified: --upgrade-policy, --capacity, --vm-size, --overprovision, --enable-auto-os-upgrade, --scale-in-policy, or --tags.",
                HttpStatusCode.BadRequest);
        }

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

            context.Response.Results = ResponseResult.Create(
                new VmssUpdateCommandResult(result),
                ComputeJsonContext.Default.VmssUpdateCommandResult);
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
