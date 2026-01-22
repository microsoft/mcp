// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Options;
using Azure.Mcp.Tools.Compute.Options.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Compute.Commands.Disk;

/// <summary>
/// Command to get details of an Azure managed disk.
/// </summary>
public sealed class DiskGetCommand(
    ILogger<DiskGetCommand> logger,
    IComputeService computeService)
    : BaseComputeCommand<DiskGetOptions>
{
    private const string CommandTitle = "Get Disk Details";
    private const string CommandDescription = "Retrieves detailed information about Azure managed disks. If a specific disk name is provided, returns details for that disk. If no disk name is provided, returns all disks in the specified resource group or subscription.";

    private readonly ILogger<DiskGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComputeService _computeService = computeService ?? throw new ArgumentNullException(nameof(computeService));

    /// <inheritdoc/>
    public override string Id => "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d";

    /// <inheritdoc/>
    public override string Name => "get";

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override string Description => CommandDescription;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = false,
        Idempotent = true,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    /// <inheritdoc/>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ComputeOptionDefinitions.Disk.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    /// <inheritdoc/>
    protected override DiskGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Disk = parseResult.GetValueOrDefault<string>(ComputeOptionDefinitions.Disk.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            if (!string.IsNullOrEmpty(options.Disk))
            {
                // Get specific disk
                if (string.IsNullOrEmpty(options.ResourceGroup))
                {
                    throw new ArgumentException("Resource group is required when retrieving a specific disk by name.");
                }

                _logger.LogInformation("Getting disk {DiskName} in resource group {ResourceGroup}", options.Disk, options.ResourceGroup);

                var disk = await _computeService.GetDiskAsync(
                    options.Disk,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new DiskGetCommandResult([disk]), ComputeJsonContext.Default.DiskGetCommandResult);
            }
            else
            {
                // List all disks
                _logger.LogInformation("Listing disks in subscription {Subscription}, resource group {ResourceGroup}", options.Subscription, options.ResourceGroup);

                var disks = await _computeService.ListDisksAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new DiskGetCommandResult(disks ?? []), ComputeJsonContext.Default.DiskGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting disks. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <inheritdoc/>
    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Azure.Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    /// <inheritdoc/>
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Disk not found. Verify the disk name and resource group are correct and you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the disk. Details: {reqEx.Message}",
        Azure.Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'az login' to sign in.",
        ArgumentException argEx =>
            $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    /// <summary>
    /// Result record for the disk get command.
    /// </summary>
    public record DiskGetCommandResult(List<Models.Disk> Disks);
}
