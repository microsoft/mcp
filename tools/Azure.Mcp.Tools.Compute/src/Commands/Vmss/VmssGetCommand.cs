// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssGetCommand(ILogger<VmssGetCommand> logger, IComputeService computeService)
    : BaseComputeCommand<VmssGetOptions>(false)
{
    private const string CommandTitle = "Get Virtual Machine Scale Set(s)";
    private readonly ILogger<VmssGetCommand> _logger = logger;
    private readonly IComputeService _computeService = computeService;

    public override string Id => "a5e2f7i9-8j6h-8e0i-2g1f-3h6i7j8e9f0g";

    public override string Name => "get";

    public override string Description =>
        """
        List or get Azure Virtual Machine Scale Sets (VMSS) and their instances in a subscription or resource group. Returns scale set details including name, location, SKU, capacity, upgrade policy, and individual VM instance information.
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add optional vmss-name
        command.Options.Add(ComputeOptionDefinitions.VmssName);

        // Add optional instance-id
        command.Options.Add(ComputeOptionDefinitions.InstanceId);
        command.Validators.Add(commandResult =>
        {
            var vmssName = commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);
            // Custom validation: If vmss-name is specified, resource-group is required (can't get specific VMSS without resource-group)
            if (!string.IsNullOrEmpty(vmssName) &&
                string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name)))
            {
                commandResult.AddError("The --resource-group option is required when retrieving a specific VMSS with --vmss-name.");
            }

            // Custom validation: If instance-id is specified, vmss-name is required
            if (!string.IsNullOrEmpty(commandResult.GetValueOrDefault<string>(ComputeOptionDefinitions.InstanceId.Name)) &&
                string.IsNullOrEmpty(vmssName))
            {
                commandResult.AddError("When --instance-id is specified, --vmss-name is required.");
            }
        });
    }

    protected override VmssGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);
        options.InstanceId = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.InstanceId.Name);
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
            // Scenario 1: Get specific VM instance in VMSS
            if (!string.IsNullOrEmpty(options.InstanceId))
            {
                var vmInstance = await _computeService.GetVmssVmAsync(
                    options.VmssName!,
                    options.InstanceId,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(vmInstance), ComputeJsonContext.Default.VmssGetVmInstanceResult);
            }
            // Scenario 2: Get specific VMSS
            else if (!string.IsNullOrEmpty(options.VmssName))
            {
                var vmss = await _computeService.GetVmssAsync(
                    options.VmssName,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(vmss), ComputeJsonContext.Default.VmssGetSingleResult);
            }
            // Scenario 3: List VMSS in resource group
            else
            {
                var vmssList = await _computeService.ListVmssAsync(
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(vmssList ?? []), ComputeJsonContext.Default.VmssGetListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving VMSS. VmssName: {VmssName}, InstanceId: {InstanceId}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.VmssName, options.InstanceId, options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine scale set or instance not found. Verify the VMSS name, instance ID, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing virtual machine scale set(s). Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssGetSingleResult(VmssInfo Vmss);
    internal record VmssGetListResult(List<VmssInfo> VmssList);
    internal record VmssGetVmInstanceResult(VmssVmInfo VmInstance);
}
