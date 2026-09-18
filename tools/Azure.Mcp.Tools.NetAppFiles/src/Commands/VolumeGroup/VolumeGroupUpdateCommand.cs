// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
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
    Id = "607ed077-045d-4faa-9346-8888baf59085",
    Name = "update",
    Title = "Update Azure NetApp Files Volume Group",
    Description = "Updates application metadata or replaces the complete member-volume specification list for an Azure NetApp Files application volume group. Returns the updated volume group details and member volume names.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VolumeGroupUpdateCommand(
    ILogger<VolumeGroupUpdateCommand> logger,
    INetAppFilesVolumeGroupService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeGroupUpdateOptions, VolumeGroupUpdateCommand.VolumeGroupUpdateResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeGroupUpdateCommand> _logger = logger;
    private readonly INetAppFilesVolumeGroupService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        VolumeGroupUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var volumes = options.Volumes is null
                ? null
                : JsonSerializer.Deserialize(
                    options.Volumes,
                    NetAppFilesJsonContext.Default.ListNetAppFilesVolumeGroupVolumeSpecification);

            var volumeGroup = await _service.UpdateVolumeGroupAsync(
                options.Account,
                options.VolumeGroup,
                options.ApplicationType,
                options.ApplicationIdentifier,
                options.GroupDescription,
                volumes,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeGroupUpdateResult(volumeGroup),
                NetAppFilesJsonContext.Default.VolumeGroupUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Azure NetApp Files volume group. Account: {Account}, VolumeGroup: {VolumeGroup}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.VolumeGroup,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(VolumeGroupUpdateOptions options, ValidationResult validationResult)
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

        if (options.ApplicationType is null &&
            options.ApplicationIdentifier is null &&
            options.GroupDescription is null &&
            options.Volumes is null)
        {
            validationResult.Errors.Add("At least one update property must be provided: --application-type, --application-identifier, --group-description, or --volumes.");
        }

        if (options.ApplicationType is not null)
        {
            VolumeGroupOptionsValidator.ValidateApplicationType(options.ApplicationType, validationResult);
        }

        if (options.ApplicationIdentifier is not null && string.IsNullOrWhiteSpace(options.ApplicationIdentifier))
        {
            validationResult.Errors.Add("--application-identifier cannot be empty or whitespace.");
        }

        if (options.Volumes is not null)
        {
            VolumeGroupOptionsValidator.ValidateVolumes(options.Volumes, validationResult);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files volume group could not be updated because of a resource conflict. Verify the volume group state and update values.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Azure NetApp Files volume group. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files volume group not found. Verify the account, volume group, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record VolumeGroupUpdateResult(NetAppFilesVolumeGroup VolumeGroup);
}
