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

public sealed class VmssGetCommand(ILogger<VmssGetCommand> logger)
    : BaseComputeCommand<VmssGetOptions>()
{
    private const string CommandTitle = "Get Virtual Machine Scale Set Details";
    private readonly ILogger<VmssGetCommand> _logger = logger;

    public override string Id => "a5e2f7i9-8j6h-8e0i-2g1f-3h6i7j8e9f0g";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about an Azure Virtual Machine Scale Set, including name, location, SKU, capacity, provisioning state, upgrade policy, overprovision settings, zones, and tags.
        Use this command to get comprehensive details about a specific VMSS in a resource group.
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

    protected override VmssGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);
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

            var vmss = await computeService.GetVmssAsync(
                options.VmssName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(vmss), ComputeJsonContext.Default.VmssGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting VMSS details. VmssName: {VmssName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
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
            $"Authorization failed accessing the virtual machine scale set. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssGetCommandResult(VmssInfo Vmss);
}
