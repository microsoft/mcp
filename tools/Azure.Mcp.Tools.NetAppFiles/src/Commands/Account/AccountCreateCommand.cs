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
    Id = "a63649c2-caaa-4744-a35b-6a67db0527b7",
    Name = "create",
    Title = "Create Azure NetApp Files Account",
    Description = "Creates an Azure NetApp Files account in a specified resource group and region. Returns the account name, resource ID, location, and provisioning state.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class AccountCreateCommand(
    ILogger<AccountCreateCommand> logger,
    INetAppFilesAccountService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AccountCreateOptions, AccountCreateCommand.AccountCreateResult>(subscriptionResolver)
{
    private readonly ILogger<AccountCreateCommand> _logger = logger;
    private readonly INetAppFilesAccountService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        AccountCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var account = await _service.CreateAccountAsync(
                options.Account,
                options.Location,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new AccountCreateResult(account),
                NetAppFilesJsonContext.Default.AccountCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files account. Account: {Account}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}",
                options.Account,
                options.ResourceGroup,
                options.Location,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(AccountCreateOptions options, ValidationResult validationResult)
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
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files account could not be created because of a resource conflict. Verify the account name and resource state.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files account. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify the resource group exists and you have access.",
        _ => base.GetErrorMessage(ex)
    };

    public record AccountCreateResult(NetAppFilesAccount Account);
}