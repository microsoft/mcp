// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;

[CommandMetadata(
    Id = "df9b3378-cab1-4ae8-b54e-522bae44e185",
    Name = "get",
    Title = "Get Azure NetApp Files Snapshot Policy",
    Description = "Gets an Azure NetApp Files snapshot policy by name from an account. Requires the account, snapshot policy name, resource group, and subscription. Returns the policy details, including its state, schedules, and tags.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SnapshotPolicyGetCommand(
    ILogger<SnapshotPolicyGetCommand> logger,
    INetAppFilesSnapshotPolicyService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SnapshotPolicyGetOptions, SnapshotPolicyGetCommand.SnapshotPolicyGetResult>(subscriptionResolver)
{
    private readonly ILogger<SnapshotPolicyGetCommand> _logger = logger;
    private readonly INetAppFilesSnapshotPolicyService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        SnapshotPolicyGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var policy = await _service.GetSnapshotPolicyAsync(
                options.Account,
                options.SnapshotPolicy,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new SnapshotPolicyGetResult(policy),
                NetAppFilesJsonContext.Default.SnapshotPolicyGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving Azure NetApp Files snapshot policy. Account: {Account}, SnapshotPolicy: {SnapshotPolicy}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.SnapshotPolicy,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(SnapshotPolicyGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!NetAppFilesChildResourceNameValidator.IsValid(options.SnapshotPolicy))
        {
            validationResult.Errors.Add("--snapshot-policy must be 1-64 characters, start and end with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed retrieving the Azure NetApp Files snapshot policy. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group, Azure NetApp Files account, or snapshot policy was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record SnapshotPolicyGetResult(NetAppFilesSnapshotPolicy SnapshotPolicy);
}
