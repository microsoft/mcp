// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vm;

public sealed class VmDeleteCommand(ILogger<VmDeleteCommand> logger)
    : BaseComputeCommand<VmDeleteOptions>(true)
{
    private const string CommandTitle = "Delete Virtual Machine";
    private readonly ILogger<VmDeleteCommand> _logger = logger;

    public override string Id => "d4e2c8a1-6f3b-4d9e-b8c7-1a2e3f4d5e6f";

    public override string Name => "delete";

    public override string Description =>
        """
        Delete, remove, or destroy an Azure Virtual Machine (VM).
        Use this to permanently remove a VM that is no longer needed.
        Equivalent to 'az vm delete'. This operation is irreversible and the VM data will be lost.
        Use --force-deletion to force delete the VM even if it is in a running or failed state
        (passes forceDeletion=true to the Azure API).
        Associated resources like disks, NICs, and public IPs are NOT automatically deleted.
        Do not use this to delete Virtual Machine Scale Sets (use VMSS delete instead).
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Required options
        command.Options.Add(ComputeOptionDefinitions.VmName.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.ForceDeletion);
    }

    protected override VmDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmName.Name);
        options.ForceDeletion = parseResult.GetValueOrDefault(ComputeOptionDefinitions.ForceDeletion);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        var computeService = context.GetService<IComputeService>();

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            await computeService.DeleteVmAsync(
                options.VmName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.ForceDeletion ? true : null,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VmDeleteCommandResult(
                    $"Virtual machine '{options.VmName}' was successfully deleted from resource group '{options.ResourceGroup}'.",
                    true),
                ComputeJsonContext.Default.VmDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting VM. VmName: {VmName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.VmName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to delete the VM. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Operation conflict. The VM may be in a state that prevents deletion. Try using --force-deletion. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmDeleteCommandResult(string Message, bool Success);
}
