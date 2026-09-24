// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;

[CommandMetadata(
    Id = "3600aed4-6b00-442f-b7c6-74c7bcfc49d6",
    Name = "get",
    Title = "Get Azure NetApp Files Backup Policy",
    Description = "Gets an Azure NetApp Files backup policy by name from an account. Requires the account, backup policy, resource group, and subscription. Returns the backup policy details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupPolicyGetCommand(
    ILogger<BackupPolicyGetCommand> logger,
    INetAppFilesBackupPolicyService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupPolicyGetOptions, BackupPolicyGetCommand.BackupPolicyGetResult>(subscriptionResolver)
{
    private readonly ILogger<BackupPolicyGetCommand> _logger = logger;
    private readonly INetAppFilesBackupPolicyService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupPolicyGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backupPolicy = await _service.GetBackupPolicyAsync(
                options.Account,
                options.BackupPolicy,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupPolicyGetResult(backupPolicy),
                NetAppFilesJsonContext.Default.BackupPolicyGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Azure NetApp Files backup policy. Account: {Account}, BackupPolicy: {BackupPolicy}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.BackupPolicy,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupPolicyGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!BackupPolicyNameValidator.IsValid(options.BackupPolicy))
        {
            validationResult.Errors.Add(BackupPolicyNameValidator.ErrorMessage);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the Azure NetApp Files backup policy. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group, Azure NetApp Files account, or backup policy was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupPolicyGetResult(NetAppFilesBackupPolicy BackupPolicy);
}