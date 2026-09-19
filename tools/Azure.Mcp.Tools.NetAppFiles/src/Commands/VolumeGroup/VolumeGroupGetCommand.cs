// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;

[CommandMetadata(
    Id = "0284180e-56a6-47b0-9f18-7baaf642b724",
    Name = "get",
    Title = "Get Azure NetApp Files Volume Group",
    Description = "Gets an application volume group by name from an Azure NetApp Files account. Returns the volume group details and member volume names.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VolumeGroupGetCommand(
    ILogger<VolumeGroupGetCommand> logger,
    INetAppFilesVolumeGroupService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeGroupGetOptions, VolumeGroupGetCommand.VolumeGroupGetResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeGroupGetCommand> _logger = logger;
    private readonly INetAppFilesVolumeGroupService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        VolumeGroupGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var volumeGroup = await _service.GetVolumeGroupAsync(
                options.Account,
                options.VolumeGroup,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeGroupGetResult(volumeGroup),
                NetAppFilesJsonContext.Default.VolumeGroupGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Azure NetApp Files volume group. Account: {Account}, VolumeGroup: {VolumeGroup}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.VolumeGroup,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(VolumeGroupGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!VolumeGroupNameValidator.IsValid(options.VolumeGroup))
        {
            validationResult.Errors.Add(VolumeGroupNameValidator.ErrorMessage);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the Azure NetApp Files volume group. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files volume group not found. Verify the account, volume group, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record VolumeGroupGetResult(NetAppFilesVolumeGroup VolumeGroup);
}