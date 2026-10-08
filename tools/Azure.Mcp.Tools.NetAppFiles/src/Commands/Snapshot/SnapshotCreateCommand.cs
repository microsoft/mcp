// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;

[CommandMetadata(
    Id = "d92d8bf1-4dd9-4b00-9a9c-ff42d6fdce01",
    Name = "create",
    Title = "Create Azure NetApp Files Snapshot",
    Description = "Creates a snapshot of an Azure NetApp Files volume. Requires the account, capacity pool, volume, snapshot, resource group, and subscription. Returns the created snapshot details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotCreateCommand(
    ILogger<SnapshotCreateCommand> logger,
    INetAppFilesSnapshotService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotCreateOptions, SnapshotCreateCommand.SnapshotCreateResult>(subscriptionResolver)
{
    private readonly ILogger<SnapshotCreateCommand> _logger = logger;
    private readonly INetAppFilesSnapshotService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        SnapshotCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await _service.CreateSnapshotAsync(
                options.Account,
                options.Pool,
                options.Volume,
                options.Snapshot,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotCreateResult(snapshot),
                NetAppFilesJsonContext.Default.SnapshotCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files snapshot. Account: {Account}, Pool: {Pool}, Volume: {Volume}, Snapshot: {Snapshot}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.Pool,
                options.Volume,
                options.Snapshot,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(SnapshotCreateOptions options, ValidationResult validationResult)
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

        if (!SnapshotNameValidator.IsValid(options.Snapshot))
        {
            validationResult.Errors.Add(SnapshotNameValidator.ErrorMessage);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files snapshot could not be created because of a resource conflict. Verify the account, capacity pool, volume, and snapshot state.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files snapshot. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group, Azure NetApp Files account, capacity pool, or volume was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record SnapshotCreateResult(NetAppFilesSnapshot Snapshot);
}