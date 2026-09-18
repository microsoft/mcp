// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Account;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Account;

[CommandMetadata(
    Id = "3d2ccbae-3d80-4be3-8f48-d951abfb0bb5",
    Name = "update",
    Title = "Update Azure NetApp Files Account",
    Description = "Updates the tags or NFSv4 user ID mapping domain of an Azure NetApp Files account. Returns the updated account name, resource ID, location, and provisioning state.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AccountUpdateCommand(
    ILogger<AccountUpdateCommand> logger,
    INetAppFilesAccountService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AccountUpdateOptions, AccountUpdateCommand.AccountUpdateResult>(subscriptionResolver)
{
    private readonly ILogger<AccountUpdateCommand> _logger = logger;
    private readonly INetAppFilesAccountService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AccountUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = options.Tags is null
                ? null
                : JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);

            var account = await _service.UpdateAccountAsync(
                options.Account,
                options.ResourceGroup,
                options.Subscription!,
                tags,
                options.NfsV4IdDomain,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new AccountUpdateResult(account),
                NetAppFilesJsonContext.Default.AccountUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Azure NetApp Files account. Account: {Account}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(AccountUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (options.Tags is null && options.NfsV4IdDomain is null)
        {
            validationResult.Errors.Add("At least one update property must be provided: --tags or --nfs-v4-id-domain.");
        }

        if (options.Tags is not null)
        {
            try
            {
                if (JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString) is null)
                {
                    validationResult.Errors.Add("--tags must be a JSON key-value object.");
                }
            }
            catch (JsonException)
            {
                validationResult.Errors.Add("--tags must be a JSON key-value object with string values.");
            }
        }

        if (options.NfsV4IdDomain is not null && string.IsNullOrWhiteSpace(options.NfsV4IdDomain))
        {
            validationResult.Errors.Add("--nfs-v4-id-domain cannot be empty or whitespace.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files account could not be updated because of a resource conflict. Verify the account state and update values.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Azure NetApp Files account. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files account not found. Verify the account and resource group names and that you have access.",
        _ => base.GetErrorMessage(ex)
    };

    public record AccountUpdateResult(NetAppFilesAccount Account);
}