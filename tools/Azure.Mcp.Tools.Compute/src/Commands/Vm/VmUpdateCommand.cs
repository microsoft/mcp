// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmUpdateCommand(ILogger<VmUpdateCommand> logger)
    : BaseComputeCommand<VmUpdateOptions>()
{
    private const string CommandTitle = "Update Virtual Machine";
    private readonly ILogger<VmUpdateCommand> _logger = logger;

    public override string Id => "g7f2e5h0-8i6d-7e1h-2f4g-3h5i6j7k8l9m";

    public override string Name => "update";

    public override string Description =>
        """
        Update an existing Azure Virtual Machine (VM) configuration.
        Supports updating VM size, tags, license type, boot diagnostics, and user data.
        Uses PATCH semantics - only specified properties are updated.

        Updatable properties:
        - --vm-size: Change the VM SKU size (requires VM to be deallocated for most size changes)
        - --tags: Add or update tags in key=value,key2=value2 format
        - --license-type: Set Azure Hybrid Benefit license type
        - --boot-diagnostics: Enable or disable boot diagnostics ('true' or 'false')
        - --user-data: Update base64-encoded user data

        Required options:
        - --vm-name: Name of the VM to update
        - --resource-group: Resource group name
        - --subscription: Subscription ID or name

        At least one update property must be specified.

        Examples:
        - Add tags: --tags environment=prod,team=compute
        - Enable Hybrid Benefit: --license-type Windows_Server
        - Disable Hybrid Benefit: --license-type None
        - Enable boot diagnostics: --boot-diagnostics true
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
        command.Options.Add(ComputeOptionDefinitions.VmName.AsRequired());

        // Update options (at least one required - validated in command)
        command.Options.Add(ComputeOptionDefinitions.VmSize);
        command.Options.Add(ComputeOptionDefinitions.Tags);
        command.Options.Add(ComputeOptionDefinitions.LicenseType);
        command.Options.Add(ComputeOptionDefinitions.BootDiagnostics);
        command.Options.Add(ComputeOptionDefinitions.UserData);

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

    protected override VmUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
        options.VmSize = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmSize.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Tags.Name);
        options.LicenseType = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.LicenseType.Name);
        options.BootDiagnostics = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.BootDiagnostics.Name);
        options.UserData = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.UserData.Name);
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
        if (string.IsNullOrEmpty(options.VmSize) &&
            string.IsNullOrEmpty(options.Tags) &&
            string.IsNullOrEmpty(options.LicenseType) &&
            string.IsNullOrEmpty(options.BootDiagnostics) &&
            string.IsNullOrEmpty(options.UserData))
        {
            throw new CommandValidationException(
                "At least one update property must be specified: --vm-size, --tags, --license-type, --boot-diagnostics, or --user-data.",
                HttpStatusCode.BadRequest);
        }

        var computeService = context.GetService<IComputeService>();

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var result = await computeService.UpdateVmAsync(
                options.VmName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VmSize,
                options.Tags,
                options.LicenseType,
                options.BootDiagnostics,
                options.UserData,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VmUpdateCommandResult(result),
                ComputeJsonContext.Default.VmUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating VM. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.VmName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "VM not found. Verify the VM name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to update VM. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Operation conflict. The VM may need to be deallocated for size changes. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase) =>
            $"Quota exceeded. You may need to request a quota increase for the selected VM size. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmUpdateCommandResult(VmUpdateResult Vm);
}
