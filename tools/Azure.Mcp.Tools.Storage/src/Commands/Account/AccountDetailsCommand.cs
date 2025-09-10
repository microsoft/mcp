// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Options.Account;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Account;

public sealed class AccountDetailsCommand(ILogger<AccountDetailsCommand> logger) : BaseStorageCommand<AccountDetailsOptions>()
{
    private const string CommandTitle = "Get Storage Account Details";
    private readonly ILogger<AccountDetailsCommand> _logger = logger;

    public override string Name => "details";

    public override string Description =>
        """
        Get detailed information about a specific Azure Storage account. This command retrieves comprehensive
        metadata for the specified storage account including name, location, SKU, access settings, and configuration
        details. Returns a JSON object with all storage account properties.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Get the storage service from DI
            var storageService = context.GetService<IStorageService>();

            // Call service operation with required parameters
            var account = await storageService.GetStorageAccountDetails(
                options.Account!,  // Required parameter ends with !
                options.Subscription!,  // From SubscriptionCommand
                options.Tenant,    // From GlobalCommand
                options.RetryPolicy);    // From GlobalCommand

            // Set results
            context.Response.Results = ResponseResult.Create(
                new AccountDetailsCommandResult(account),
                StorageJsonContext.Default.AccountDetailsCommandResult);
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error getting storage account details. Account: {Account}, Subscription: {Subscription}, Options: {@Options}",
                options.Account, options.Subscription, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "Storage account not found. Verify the account name exists and you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the storage account. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record AccountDetailsCommandResult(Models.StorageAccountInfo Account);
}
