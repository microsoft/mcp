// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmListCommand(ILogger<VmListCommand> logger)
    : BaseComputeCommand<VmListOptions>()
{
    private const string CommandTitle = "List Virtual Machines";
    private readonly ILogger<VmListCommand> _logger = logger;

    public override string Id => "d2b9c4f6-5g3e-5b7f-9d8c-0e3f4g5b6c7d";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all virtual machines in a resource group or subscription. Returns comprehensive information about each VM including name, location, VM size, provisioning state, OS type, zones, and tags.
        Use this command to discover and inventory VMs in your Azure environment.
        If resource-group is specified, lists VMs in that group; otherwise lists all VMs in the subscription.
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
            Description = "The name of the resource group (optional - if not specified, lists all VMs in subscription)"
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

            var vms = await computeService.ListVmsAsync(
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(vms ?? []), ComputeJsonContext.Default.VmListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing VMs. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
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
            $"Authorization failed accessing virtual machines. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmListCommandResult(List<VmInfo> Vms);
}
