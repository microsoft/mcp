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

public sealed class VmssVmsListCommand(ILogger<VmssVmsListCommand> logger)
    : BaseComputeCommand<VmssVmsListOptions>()
{
    private const string CommandTitle = "List VM Instances in Scale Set";
    private readonly ILogger<VmssVmsListCommand> _logger = logger;

    public override string Id => "c7g4h9k1-0l8j-0g2k-4i3h-5j8k9l0g1h2i";

    public override string Name => "vms-list";

    public override string Description =>
        """
        Lists all virtual machine instances in a virtual machine scale set. Returns detailed information about each VM instance including instance ID, name, location, VM size, provisioning state, OS type, zones, and tags.
        Use this command to view all instances within a specific scale set and their individual states.
        Required parameters: subscription, resource-group, vmss-name.
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
    }

    protected override VmssVmsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssNameName);
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

            var vms = await computeService.ListVmssVmsAsync(
                options.VmssName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(vms ?? []), ComputeJsonContext.Default.VmssVmsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing VMSS VMs. VmssName: {VmssName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.VmssName, options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine scale set not found. Verify the VMSS name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the virtual machine scale set instances. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssVmsListCommandResult(List<VmssVmInfo> Vms);
}
