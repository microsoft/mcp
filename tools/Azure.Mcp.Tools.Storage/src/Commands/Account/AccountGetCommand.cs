// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Pagination;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Services.Caching.Pagination;

namespace Azure.Mcp.Tools.Storage.Commands.Account;

public sealed class AccountGetCommand : SubscriptionCommand<AccountGetOptions>
{
    private const string CommandTitle = "Get Storage Account Details";
    private const string OperationName = "storage.account.get";
    private readonly ILogger<AccountGetCommand> _logger;
    private readonly IStorageService _storageService;
    private readonly IPaginationService? _paginationService;

    public AccountGetCommand(ILogger<AccountGetCommand> logger, IStorageService storageService)
        : this(logger, storageService, null)
    {
    }

    public AccountGetCommand(ILogger<AccountGetCommand> logger, IStorageService storageService, IPaginationService? paginationService)
    {
        _logger = logger;
        _storageService = storageService;
        _paginationService = paginationService;
    }

    public override string Id => "eb2363f1-f21f-45fc-ad63-bacfbae8c45c";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves detailed information about Azure Storage accounts, including account name, location, SKU, kind, hierarchical namespace status, HTTPS-only settings, and blob public access configuration. If a specific account name is not provided, the command will return details for all accounts in a subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false,
        SupportsPagination = _paginationService is not null
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Account.AsOptional());
        command.Options.Add(OptionDefinitions.Pagination.Cursor.AsOptional());
    }

    protected override AccountGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Account.Name);
        options.Cursor = parseResult.GetValueOrDefault<string>(OptionDefinitions.Pagination.Cursor.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            if (_paginationService is not null && String.IsNullOrEmpty(options.Account))
            {
                if (context.SupportsApps)
                {
                    return await GetPagedResourceUriAsync(context, options, cancellationToken);
                }

                return await GetPagedResultsAsync(context, options, cancellationToken);
            }

            // Original path — call GetAccountDetails as before
            var accounts = await _storageService.GetAccountDetails(
                options.Account,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new AccountGetCommandResult(accounts?.Results ?? [], accounts?.AreResultsTruncated ?? false),
                StorageJsonContext.Default.AccountGetCommandResult);
        }
        catch (Exception ex)
        {
            if (options.Account is null)
            {
                _logger.LogError(ex, "Error listing account details. Subscription: {Subscription}, Options: {@Options}", options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex, "Error getting storage account details. Account: {Account}, Subscription: {Subscription}, Options: {@Options}",
                    options.Account, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    private async Task<CommandResponse>  GetPagedResourceUriAsync(CommandContext context, AccountGetOptions options, CancellationToken cancellationToken)
    {
        var fingerprint = ComputeFingerprint(options);

        PageFetchDelegate fetcher = async (nativeState, ct) =>
        {
            var adapter = new KqlPaginationAdapter<StorageAccountInfo>(
                skipToken => _storageService.GetAccountDetailsPaged(
                    options.Subscription!, options.Tenant, options.RetryPolicy, skipToken, ct));

            var page = nativeState is null
                ? await adapter.FetchFirstPageAsync(ct)
                : await adapter.FetchNextPageAsync(nativeState, ct);

            var itemsJson = JsonSerializer.Serialize(page.Items, StorageJsonContext.Default.ListStorageAccountInfo);
            return new PaginationPageData(itemsJson, page.NativeState);
        };

        // Use a sentinel native state to indicate the cursor starts at page 1
        var cursorId = await _paginationService!.SaveCursorAsync(
            KqlPaginationAdapter<StorageAccountInfo>.ProviderName, OperationName,
            fingerprint, PaginationResource.InitialNativeState,
            fetcher: fetcher,
            cancellationToken: cancellationToken);

        var resourceUri = $"{PaginationResource.UriPrefix}{cursorId}";

        context.Response.Results = ResponseResult.Create(
            new AccountGetResourceResult(resourceUri, new ResponseMeta(new ResponseMetaUi(TableAppResource.UriPrefix))),
            StorageJsonContext.Default.AccountGetResourceResult);

        return context.Response;
    }

    private async Task<CommandResponse> GetPagedResultsAsync(CommandContext context, AccountGetOptions options, CancellationToken cancellationToken)
    {
        var fingerprint = ComputeFingerprint(options);

        var adapter = new KqlPaginationAdapter<StorageAccountInfo>(
            skipToken => _storageService.GetAccountDetailsPaged(
                options.Subscription!, options.Tenant, options.RetryPolicy, skipToken, cancellationToken));

        PageResult<StorageAccountInfo>? pagedResult = null;
        if (String.IsNullOrEmpty(options.Cursor))
        {
            pagedResult = await adapter.FetchFirstPageAsync(cancellationToken);
        }
        else
        {
            var cursorRecord = await _paginationService!.LoadAndValidateCursorAsync(
                options.Cursor!, fingerprint, cancellationToken);
            pagedResult = await adapter.FetchNextPageAsync(cursorRecord.NativeState, cancellationToken);
        }

        string? nextCursor = null;
        if (pagedResult.NativeState is not null)
        {
            nextCursor = await _paginationService!.SaveCursorAsync(
                KqlPaginationAdapter<StorageAccountInfo>.ProviderName, OperationName,
                fingerprint, pagedResult.NativeState,
                cancellationToken: cancellationToken);
        }

        context.Response.Results = ResponseResult.Create(
            new AccountGetCommandResult(pagedResult.Items, false, nextCursor),
            StorageJsonContext.Default.AccountGetCommandResult);

        return context.Response;
    }

    private string ComputeFingerprint(AccountGetOptions options) =>
        _paginationService!.ComputeRequestFingerprint(new Dictionary<string, string?>
        {
            ["operation"] = OperationName,
            ["subscription"] = options.Subscription,
        });

    // Strongly-typed result record — NextCursor is additive and null by default
    internal record AccountGetCommandResult(List<StorageAccountInfo> Accounts, bool AreResultsTruncated, string? NextCursor = null);

    // Result when client supports resource-based pagination
    internal record AccountGetResourceResult(string PagedResourceUri, [property: JsonPropertyName("_meta")] ResponseMeta? Meta = null);

    internal record ResponseMeta(ResponseMetaUi? Ui = null);

    internal record ResponseMetaUi(string? ResourceUri = null);
}
