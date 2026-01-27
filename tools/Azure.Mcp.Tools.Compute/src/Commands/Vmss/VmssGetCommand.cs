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
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssGetCommand(ILogger<VmssGetCommand> logger)
    : BaseComputeCommand<VmssGetOptions>()
{
    private const string CommandTitle = "Get Virtual Machine Scale Set(s)";
    private readonly ILogger<VmssGetCommand> _logger = logger;

    public override string Id => "a5e2f7i9-8j6h-8e0i-2g1f-3h6i7j8e9f0g";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves information about Azure Virtual Machine Scale Set(s) and their VM instances. Behavior depends on provided parameters:
        - With --instance-id: Gets detailed information about a specific VM instance in a scale set (requires --vmss-name and --resource-group).
        - With --vmss-name: Gets detailed information about a specific VMSS (requires --resource-group).
        - With --resource-group only: Lists all scale sets in the specified resource group.
        - With neither: Lists all scale sets in the subscription.
        Returns VMSS information including name, location, SKU, capacity, provisioning state, upgrade policy, zones, and tags.
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

        // Make resource-group optional for listing scenarios
        command.Options.Remove(OptionDefinitions.Common.ResourceGroup);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());

        // Add optional vmss-name
        command.Options.Add(ComputeOptionDefinitions.VmssName);

        // Add optional instance-id
        command.Options.Add(ComputeOptionDefinitions.InstanceId);
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

        // Custom validation: If instance-id is specified, vmss-name and resource-group are required
        if (!string.IsNullOrEmpty(options.InstanceId))
        {
            if (string.IsNullOrEmpty(options.VmssName))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "When --instance-id is specified, --vmss-name is required.";
                return context.Response;
            }
            if (string.IsNullOrEmpty(options.ResourceGroup))
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = "When --instance-id is specified, --resource-group is required.";
                return context.Response;
            }
        }

        // Custom validation: If vmss-name is specified, resource-group is required
        if (!string.IsNullOrEmpty(options.VmssName) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "When --vmss-name is specified, --resource-group is required.";
            return context.Response;
        }

        var computeService = context.GetService<IComputeService>();

        try
        {
            // Scenario 1: Get specific VM instance in VMSS
            if (!string.IsNullOrEmpty(options.InstanceId))
            {
                var vmInstance = await computeService.GetVmssVmAsync(
                    options.VmssName!,
                    options.InstanceId,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new VmssGetVmInstanceResult(vmInstance),
                    ComputeJsonContext.Default.VmssGetVmInstanceResult);
            }
            // Scenario 2: Get specific VMSS
            else if (!string.IsNullOrEmpty(options.VmssName))
            {
                var vmss = await computeService.GetVmssAsync(
                    options.VmssName,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new VmssGetSingleResult(vmss),
                    ComputeJsonContext.Default.VmssGetSingleResult);
            }
            // Scenario 3 & 4: List VMSS (in resource group or subscription)
            else
            {
                var vmssList = await computeService.ListVmssAsync(
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new VmssGetListResult(vmssList ?? []),
                    ComputeJsonContext.Default.VmssGetListResult);
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
