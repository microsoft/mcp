// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    Id = "d46bb832-5b3e-4259-85c4-5d5f70e30a4c",
    Name = "get",
    Title = "Get Azure NetApp Files Account",
    Description = "Gets an Azure NetApp Files account by name from a specified resource group. Returns the account name, resource ID, location, and provisioning state.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AccountGetCommand(
    ILogger<AccountGetCommand> logger,
    INetAppFilesAccountService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AccountGetOptions, AccountGetCommand.AccountGetResult>(subscriptionResolver)
{
    private readonly ILogger<AccountGetCommand> _logger = logger;
    private readonly INetAppFilesAccountService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AccountGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _service.GetAccountAsync(
                options.Account,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new AccountGetResult(account),
                NetAppFilesJsonContext.Default.AccountGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Azure NetApp Files account. Account: {Account}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(AccountGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Resource group not found. Verify the resource group exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the Azure NetApp Files account. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Azure NetApp Files account not found. Verify the account and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record AccountGetResult(NetAppFilesAccount Account);
}