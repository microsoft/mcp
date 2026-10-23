// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Azure;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Paging;

namespace Azure.Mcp.Tools.Storage.Commands.Account;

[CommandMetadata(
    Id = "eb2363f1-f21f-45fc-ad63-bacfbae8c45c",
    Name = "get",
    Title = "Get Storage Account Details",
    Description = "Retrieves detailed information about Azure Storage accounts, including account name, location, SKU, kind, hierarchical namespace status, HTTPS-only settings, and blob public access configuration. With an account name, returns that account without paging. Otherwise returns one page of accounts, with up to 50 accounts by default; use page-size to request 1-1000. Replay paging.continuationArguments unchanged to retrieve the next page, including on another compatible server instance. Note: this queries Azure Resource Graph, which can lag a few minutes behind the actual resource state - a storage account created immediately beforehand may not appear yet.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class AccountGetCommand(ILogger<AccountGetCommand> logger, IStorageService storageService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AccountGetOptions, AccountGetCommand.AccountGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AccountGetCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override JsonTypeInfo<AccountGetCommandResult>? ResultTypeInfo => StorageJsonContext.Default.AccountGetCommandResult;

    public override void ValidateOptions(AccountGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (options.Account is not null && (options.PageSize.HasValue || options.ContinuationToken is not null))
        {
            validationResult.Errors.Add("--page-size and --continuation-token are available only when --account is omitted.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AccountGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var accounts = await _storageService.GetAccountDetails(
                options.Account,
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                cancellationToken,
                options.Account is null ? new PageRequest(options.PageSize, options.ContinuationToken) : null);

            SetResult(context, new(accounts.Results, accounts.AreResultsTruncated)
            {
                Paging = options.Account is null
                    ? (accounts.Page ?? throw new InvalidOperationException("The service did not return resource paging information.")).ToPagingMetadata(Id)
                    : null
            });
        }
        catch (Exception ex)
        {
            if (options.Account is null)
            {
                _logger.LogError(ex, "Error listing account details. Subscription: {Subscription}.", options.Subscription);
            }
            else
            {
                _logger.LogError(ex, "Error getting storage account details. Account: {Account}, Subscription: {Subscription}.",
                    options.Account, options.Subscription);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Storage account not found. Verify the account name, subscription, and that you have access. Note: recently created accounts may take a few minutes to appear due to Azure Resource Graph indexing delay.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Access denied listing storage account details. This is commonly caused by missing 'Reader' (or higher) access on the subscription or resource group, but can also result from an Azure Policy restriction or a conditional access requirement. See https://learn.microsoft.com/azure/role-based-access-control/troubleshooting for troubleshooting steps. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record AccountGetCommandResult(List<StorageAccountInfo> Accounts, bool AreResultsTruncated)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PagingMetadata? Paging { get; init; }
    }
}
