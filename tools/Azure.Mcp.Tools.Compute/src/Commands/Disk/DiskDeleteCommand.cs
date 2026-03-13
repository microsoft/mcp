// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Compute.Commands.Disk;

/// <summary>
/// Command to delete an Azure managed disk.
/// </summary>
public sealed class DiskDeleteCommand(
    ILogger<DiskDeleteCommand> logger)
    : BaseComputeCommand<DiskDeleteOptions>(true)
{
    private const string CommandTitle = "Delete Managed Disk";
    private const string CommandDescription =
        "Deletes an Azure managed disk from the specified resource group. "
        + "This is an idempotent operation that returns Deleted = true if the disk was successfully removed, "
        + "or Deleted = false if the disk was not found. "
        + "The disk must not be attached to a virtual machine; detach it first before deleting.";

    private readonly ILogger<DiskDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc/>
    public override string Id => "a7c3e9f1-4b82-4d5a-9e6c-1f3d8b2a7c4e";

    /// <inheritdoc/>
    public override string Name => "delete";

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override string Description => CommandDescription;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = true,
        Idempotent = true,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    /// <inheritdoc/>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Disk.AsRequired());
    }

    /// <inheritdoc/>
    protected override DiskDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Disk = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Disk.Name);
        return options;
    }

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            _logger.LogInformation(
                "Deleting disk {DiskName} in resource group {ResourceGroup}",
                options.Disk, options.ResourceGroup);

            var computeService = context.GetService<IComputeService>();
            var deleted = await computeService.DeleteDiskAsync(
                options.Disk!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DiskDeleteCommandResult(deleted, options.Disk!),
                ComputeJsonContext.Default.DiskDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting disk. Disk: {Disk}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Disk, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <inheritdoc/>
    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    /// <inheritdoc/>
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 409 =>
            "The disk is currently attached to a virtual machine. Detach the disk before deleting.",
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Disk or resource group not found. Verify the disk name and resource group are correct.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed deleting the disk. Details: {reqEx.Message}",
        Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx =>
            $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the disk delete command.
    /// </summary>
    public record DiskDeleteCommandResult(bool Deleted, string DiskName);
}
