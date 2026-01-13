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

public sealed class VmssListCommand(ILogger<VmssListCommand> logger)
    : BaseComputeCommand<VmssListOptions>()
{
    private const string CommandTitle = "List Virtual Machine Scale Sets";
    private readonly ILogger<VmssListCommand> _logger = logger;

    public override string Id => "b6f3g8j0-9k7i-9f1j-3h2g-4i7j8k9f0g1h";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all virtual machine scale sets in a resource group or subscription. Returns comprehensive information about each VMSS including name, location, SKU, capacity, provisioning state, upgrade policy, zones, and tags.
        Use this command to discover and inventory scale sets in your Azure environment.
        If resource-group is specified, lists VMSS in that group; otherwise lists all VMSS in the subscription.
        Required parameter: subscription.
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
        // Make resource-group optional for list
        command.Options.Remove(ComputeOptionDefinitions.ResourceGroup);
        var optionalRg = new Option<string>(
            "--resource-group",
            "-g")
        {
            Description = "The name of the resource group (optional - if not specified, lists all VMSS in subscription)"
        };
        command.Options.Add(optionalRg);
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

            var vmssList = await computeService.ListVmssAsync(
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(vmssList ?? []), ComputeJsonContext.Default.VmssListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing VMSS. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify the resource group name and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing virtual machine scale sets. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssListCommandResult(List<VmssInfo> VmssList);
}
