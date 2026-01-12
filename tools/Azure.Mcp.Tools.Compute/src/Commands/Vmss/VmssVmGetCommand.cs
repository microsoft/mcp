// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssVmGetCommand(ILogger<VmssVmGetCommand> logger)
    : BaseComputeCommand<VmssVmGetOptions>()
{
    private const string CommandTitle = "Get Scale Set VM Instance Details";
    private readonly ILogger<VmssVmGetCommand> _logger = logger;

    public override string Id => "d8h5i0l2-1m9k-1h3l-5j4i-6k9l0m1h2i3j";

    public override string Name => "vm-get";

    public override string Description =>
        """
        Retrieves detailed information about a specific virtual machine instance in a virtual machine scale set. Returns information including instance ID, name, location, VM size, provisioning state, OS type, zones, and tags.
        Use this command to get comprehensive details about a specific VM instance within a scale set.
        Required parameters: subscription, resource-group, vmss-name, instance-id.
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
        command.Options.Add(ComputeOptionDefinitions.VmssName);
        command.Options.Add(ComputeOptionDefinitions.InstanceId);
    }

    protected override VmssVmGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssNameName);
        options.InstanceId = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.InstanceIdName);
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
            var computeService = context.GetService<IComputeService>();

            var vm = await computeService.GetVmssVmAsync(
                options.VmssName!,
                options.InstanceId!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(vm), ComputeJsonContext.Default.VmssVmGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting VMSS VM instance. VmssName: {VmssName}, InstanceId: {InstanceId}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.VmssName, options.InstanceId, options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine instance not found. Verify the VMSS name, instance ID, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the virtual machine scale set instance. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssVmGetCommandResult(VmssVmInfo Vm);
}
