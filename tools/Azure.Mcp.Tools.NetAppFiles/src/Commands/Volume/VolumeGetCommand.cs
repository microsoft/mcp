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
    Id = "d0b98e20-395a-4af5-9bb2-515fcaf6372d",
    Name = "get",
    Title = "Get Azure NetApp Files Volume",
    Description = "Gets an Azure NetApp Files volume by name from a capacity pool. Requires the account, pool, volume, resource group, and subscription. Returns the volume name, resource ID, location, provisioning state, quota in GiB, and service level.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VolumeGetCommand(
    ILogger<VolumeGetCommand> logger,
    INetAppFilesVolumeService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeGetOptions, VolumeGetCommand.VolumeGetResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeGetCommand> _logger = logger;
    private readonly INetAppFilesVolumeService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        VolumeGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var volume = await _service.GetVolumeAsync(
                options.Account,
                options.Pool,
                options.Volume,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeGetResult(volume),
                NetAppFilesJsonContext.Default.VolumeGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Azure NetApp Files volume. Account: {Account}, Pool: {Pool}, Volume: {Volume}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.Pool,
                options.Volume,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(VolumeGetOptions options, ValidationResult validationResult)
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
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the Azure NetApp Files volume. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files volume not found. Verify the account, capacity pool, volume, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record VolumeGetResult(NetAppFilesVolume Volume);
}