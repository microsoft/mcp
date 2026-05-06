// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmGetCommand(ILogger<VmGetCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmGetOptions, VmGetCommand.VmGetCommandResult>(false)
{
    private const string CommandTitle = "Get Virtual Machine(s)";
    private readonly ILogger<VmGetCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "c1a8b3e5-4f2d-4a6e-8c7b-9d2e3f4a5b6c";

    public override string Name => "get";

    public override string Description =>
        """
        List or get Azure Virtual Machine (VM) configuration and properties in a resource group. By default, returns VM details including name, location, size, provisioning state, and OS type. When retrieving a specific VM with --vm-name and --instance-view, the response also includes power state (running/stopped/deallocated). Use this tool to retrieve VM configuration details.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override JsonTypeInfo<VmGetCommandResult> ResultTypeInfo => ComputeJsonContext.Default.VmGetCommandResult;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add optional vm-name
        command.Options.Add(ComputeOptionDefinitions.VmName);

        // Add optional instance-view
        command.Options.Add(ComputeOptionDefinitions.InstanceView);
        command.Validators.Add(commandResult =>
        {
            var vmName = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
            var instanceView = commandResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.InstanceView.Name);
            var resourceGroup = commandResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

            // Custom validation: If vm-name is specified, resource-group is required (can't get specific VM without resource-group)
            if (!string.IsNullOrEmpty(vmName) && string.IsNullOrEmpty(resourceGroup))
            {
                commandResult.AddError("The --resource-group option is required when retrieving a specific VM with --vm-name.");
            }

            // Custom validation: If instance-view is specified, vm-name is required
            if (instanceView && string.IsNullOrEmpty(vmName))
            {
                commandResult.AddError("The --instance-view option is only available when retrieving a specific VM with --vm-name.");
            }
        });
    }

    protected override VmGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
        options.InstanceView = parseResult.GetValueOrDefault<bool>(ComputeOptionDefinitions.InstanceView.Name);
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
            // Scenario 1: Get specific VM with optional instance view
            if (!string.IsNullOrEmpty(options.VmName))
            {
                if (options.InstanceView)
                {
                    var vmWithInstanceView = await _computeService.GetVmWithInstanceViewAsync(
                        options.VmName,
                        options.ResourceGroup!,
                        options.Subscription!,
                        options.Tenant,
                        options.RetryPolicy,
                        cancellationToken);

                    SetResult(context, new VmGetCommandResult(Vm: vmWithInstanceView.VmInfo, InstanceView: vmWithInstanceView.InstanceView));
                }
                else
                {
                    var vm = await _computeService.GetVmAsync(
                        options.VmName,
                        options.ResourceGroup!,
                        options.Subscription!,
                        options.Tenant,
                        options.RetryPolicy,
                        cancellationToken);

                    SetResult(context, new VmGetCommandResult(Vm: vm));
                }
            }
            // Scenario 2: List VMs in resource group
            else
            {
                var vmList = await _computeService.ListVmsAsync(
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                SetResult(context, new VmGetCommandResult(VmList: vmList));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving VM(s). VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.VmName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine not found. Verify the VM name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing virtual machine(s). Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record VmGetCommandResult(
        List<VmInfo>? VmList = null,
        VmInfo? Vm = null,
        VmInstanceView? InstanceView = null);
}
