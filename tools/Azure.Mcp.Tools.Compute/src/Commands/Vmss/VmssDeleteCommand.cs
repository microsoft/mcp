// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Vmss;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Vmss;

public sealed class VmssDeleteCommand(ILogger<VmssDeleteCommand> logger)
    : BaseComputeCommand<VmssDeleteOptions>(true)
{
    private const string CommandTitle = "Delete Virtual Machine Scale Set";
    private readonly ILogger<VmssDeleteCommand> _logger = logger;

    public override string Id => "e5f3d9b2-7a4c-4e8f-c9d8-2b3f4a5e6d7c";

    public override string Name => "delete";

    public override string Description =>
        """
        Delete, remove, or destroy an Azure Virtual Machine Scale Set (VMSS) and all its VM instances.
        Use this to permanently remove a scale set that is no longer needed.
        Equivalent to 'az vmss delete'. This operation is irreversible and all VMSS instances will be lost.
        Use --force-deletion to force delete the VMSS even if it is in a running or failed state
        (passes forceDeletion=true to the Azure API).
        Do not use this to delete a single VM (use VM delete instead).
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
        command.Options.Add(ComputeOptionDefinitions.VmssName.AsRequired());
        command.Options.Add(ComputeOptionDefinitions.ForceDeletion);
    }

    protected override VmssDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VmssName = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.VmssName.Name);
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

            await computeService.DeleteVmssAsync(
                options.VmssName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.ForceDeletion ? true : null,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VmssDeleteCommandResult(
                    $"Virtual machine scale set '{options.VmssName}' was successfully deleted from resource group '{options.ResourceGroup}'.",
                    true),
                ComputeJsonContext.Default.VmssDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting VMSS. VmssName: {VmssName}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.VmssName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Verify you have appropriate permissions to delete the VMSS. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Operation conflict. The VMSS may be in a state that prevents deletion. Try using --force-deletion. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VmssDeleteCommandResult(string Message, bool Success);
}
