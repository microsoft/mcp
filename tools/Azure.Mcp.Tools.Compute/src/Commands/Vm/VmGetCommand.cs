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

public sealed class VmGetCommand(ILogger<VmGetCommand> logger)
    : BaseComputeCommand<VmGetOptions>()
{
    private const string CommandTitle = "Get Virtual Machine Details";
    private readonly ILogger<VmGetCommand> _logger = logger;

    public override string Id => "c1a8b3e5-4f2d-4a6e-8c7b-9d2e3f4a5b6c";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about an Azure Virtual Machine, including name, location, VM size, provisioning state, OS type, license type, zones, and tags.
        Use this command to get comprehensive details about a specific VM in a resource group.
        Required parameters: subscription, resource-group, vm-name.
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
        command.Options.Add(ComputeOptionDefinitions.VmName);
    }

    protected override VmGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
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

            var vm = await computeService.GetVmAsync(
                options.VmName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(vm), ComputeJsonContext.Default.VmGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting VM details. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                options.VmName, options.ResourceGroup, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Virtual machine not found. Verify the VM name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the virtual machine. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmGetCommandResult(VmInfo Vm);
}
