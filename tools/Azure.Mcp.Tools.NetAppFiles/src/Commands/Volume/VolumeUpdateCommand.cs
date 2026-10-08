// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Volume;

[CommandMetadata(
    Id = "6d5d2608-46c8-476c-8baa-98a6b2487e4a",
    Name = "update",
    Title = "Update Azure NetApp Files Volume",
    Description = "Updates the storage quota of an Azure NetApp Files volume. Requires the account, pool, volume, quota in GiB, resource group, and subscription. Returns the updated volume details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VolumeUpdateCommand(
    ILogger<VolumeUpdateCommand> logger,
    INetAppFilesVolumeService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeUpdateOptions, VolumeUpdateCommand.VolumeUpdateResult>(subscriptionResolver)
{
    private const long MinimumQuotaGib = 50;
    private const long MaximumQuotaGib = 100 * 1024;
    private readonly ILogger<VolumeUpdateCommand> _logger = logger;
    private readonly INetAppFilesVolumeService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        VolumeUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var volume = await _service.UpdateVolumeAsync(
                options.Account,
                options.Pool,
                options.Volume,
                options.QuotaGib,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeUpdateResult(volume),
                NetAppFilesJsonContext.Default.VolumeUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Azure NetApp Files volume. Account: {Account}, Pool: {Pool}, Volume: {Volume}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.Pool,
                options.Volume,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(VolumeUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!NetAppFilesChildResourceNameValidator.IsValid(options.Pool))
        {
            validationResult.Errors.Add("--pool must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.");
        }

        if (!NetAppFilesChildResourceNameValidator.IsValid(options.Volume))
        {
            validationResult.Errors.Add("--volume must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.");
        }

        if (options.QuotaGib is < MinimumQuotaGib or > MaximumQuotaGib)
        {
            validationResult.Errors.Add($"--quota-gib must be between {MinimumQuotaGib} and {MaximumQuotaGib} GiB.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files volume could not be updated because of a resource conflict. Verify the volume state and quota value.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Azure NetApp Files volume. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files volume not found. Verify the account, capacity pool, volume, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record VolumeUpdateResult(NetAppFilesVolume Volume);
}