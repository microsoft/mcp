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
    Id = "a5c30830-fc15-45ea-a0a6-84a8d620ae19",
    Name = "get",
    Title = "Get Azure NetApp Files Snapshot",
    Description = "Gets an Azure NetApp Files snapshot by name from a volume. Requires the account, capacity pool, volume, snapshot, resource group, and subscription. Returns the snapshot details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotGetCommand(
    ILogger<SnapshotGetCommand> logger,
    INetAppFilesSnapshotService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotGetOptions, SnapshotGetCommand.SnapshotGetResult>(subscriptionResolver)
{
    private readonly ILogger<SnapshotGetCommand> _logger = logger;
    private readonly INetAppFilesSnapshotService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        SnapshotGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await _service.GetSnapshotAsync(
                options.Account,
                options.Pool,
                options.Volume,
                options.Snapshot,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotGetResult(snapshot),
                NetAppFilesJsonContext.Default.SnapshotGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving Azure NetApp Files snapshot. Account: {Account}, Pool: {Pool}, Volume: {Volume}, Snapshot: {Snapshot}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
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

    public override void ValidateOptions(SnapshotGetOptions options, ValidationResult validationResult)
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
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed retrieving the Azure NetApp Files snapshot. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group, Azure NetApp Files account, capacity pool, volume, or snapshot was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record SnapshotGetResult(NetAppFilesSnapshot Snapshot);
}
