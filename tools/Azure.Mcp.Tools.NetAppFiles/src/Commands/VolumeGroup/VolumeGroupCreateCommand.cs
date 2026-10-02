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
    Id = "ba431eb8-e666-439b-9d49-7fb1eef436ae",
    Name = "create",
    Title = "Create Azure NetApp Files Volume Group",
    Description = "Creates an SAP HANA or Oracle application volume group and its member volumes in an Azure NetApp Files account. Returns the volume group details and member volume names.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VolumeGroupCreateCommand(
    ILogger<VolumeGroupCreateCommand> logger,
    INetAppFilesVolumeGroupService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VolumeGroupCreateOptions, VolumeGroupCreateCommand.VolumeGroupCreateResult>(subscriptionResolver)
{
    private readonly ILogger<VolumeGroupCreateCommand> _logger = logger;
    private readonly INetAppFilesVolumeGroupService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        VolumeGroupCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var volumes = JsonSerializer.Deserialize(
                options.Volumes,
                NetAppFilesJsonContext.Default.ListNetAppFilesVolumeGroupVolumeSpecification)!;

            var volumeGroup = await _service.CreateVolumeGroupAsync(
                options.Account,
                options.VolumeGroup,
                options.Location,
                options.ApplicationType,
                options.ApplicationIdentifier,
                volumes,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VolumeGroupCreateResult(volumeGroup),
                NetAppFilesJsonContext.Default.VolumeGroupCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files volume group. Account: {Account}, VolumeGroup: {VolumeGroup}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.VolumeGroup,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(VolumeGroupCreateOptions options, ValidationResult validationResult)
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

        VolumeGroupOptionsValidator.ValidateApplicationType(options.ApplicationType, validationResult);

        if (string.IsNullOrWhiteSpace(options.ApplicationIdentifier))
        {
            validationResult.Errors.Add("--application-identifier cannot be empty or whitespace.");
        }

        VolumeGroupOptionsValidator.ValidateVolumes(options.Volumes, validationResult);
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files volume group could not be created because of a resource conflict. Verify the account, capacity pools, and volume names.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files volume group. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "A required Azure NetApp Files or network resource was not found. Verify the account, capacity pools, subnet, and resource group.",
        _ => base.GetErrorMessage(ex)
    };

    public record VolumeGroupCreateResult(NetAppFilesVolumeGroup VolumeGroup);
}
