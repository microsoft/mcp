// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Extensions;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Account;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Account;

[CommandMetadata(
    Id = "a3c7d1e5-8b2f-4f6a-9e0d-c5b4a7f2e8d1",
    Name = "update",
    Description =
        """
        Updates an existing Azure NetApp Files account in a specified resource group, and returns the updated account details including name, location, resource group, and provisioning state. Supports updating tags. Requires account name, resource group, location, and subscription. Optionally accepts tags in JSON format.
        """,
    Title = "Update NetApp Files Account",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class AccountUpdateCommand(ILogger<AccountUpdateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AccountUpdateOptions, AccountUpdateCommand.AccountUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AccountUpdateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AccountUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ResolveResourceIdArguments(options);
            ValidateUnsupportedUpdateArguments(options);

            Dictionary<string, string>? tags = null;
            if (!string.IsNullOrEmpty(options.Tags))
            {
                try
                {
                    tags = JsonSerializer.Deserialize(options.Tags, NetAppFilesJsonContext.Default.DictionaryStringString);
                }
                catch (JsonException ex)
                {
                    throw new ArgumentException($"Invalid tags JSON format: {ex.Message}", nameof(options.Tags));
                }
            }

            JsonElement? userAssignedIdentities = ParseJsonElementOption(options.UserAssignedIdentities, nameof(options.UserAssignedIdentities));
            JsonElement? activeDirectories = ParseJsonElementOption(options.ActiveDirectories, nameof(options.ActiveDirectories));

            var account = await _netAppFilesService.UpdateAccount(
                options.Account!,
                options.ResourceGroup!,
                options.Location,
                options.Subscription!,
                tags,
                options.KeyName,
                options.KeySource,
                options.KeyVaultResourceId,
                options.KeyVaultUri,
                options.FederatedClientId,
                options.UserAssignedIdentity,
                options.IdentityType,
                userAssignedIdentities,
                activeDirectories,
                options.NfsV4IdDomain,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new AccountUpdateCommandResult(account),
                NetAppFilesJsonContext.Default.AccountUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating NetApp Files account. Account: {Account}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Account, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "An account with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the account. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Account or resource group not found. Verify they exist and you have access.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    private static JsonElement? ParseJsonElementOption(string? value, string optionName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(value, NetAppFilesJsonContext.Default.JsonElement);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format for {optionName}: {ex.Message}", optionName, ex);
        }
    }

    private static void ResolveResourceIdArguments(AccountUpdateOptions options)
    {
        if (options.Ids is { Length: > 0 })
        {
            if (options.Ids.Length > 1)
            {
                throw new ArgumentException("Only a single resource ID is supported for account update operations.", nameof(options.Ids));
            }

            var resourceIdentifier = new ResourceIdentifier(options.Ids[0]);
            options.Account = resourceIdentifier.Name;
            options.ResourceGroup = resourceIdentifier.ResourceGroupName;
            options.Subscription = resourceIdentifier.SubscriptionId;
        }

        if (string.IsNullOrEmpty(options.Account) || string.IsNullOrEmpty(options.ResourceGroup))
        {
            throw new ArgumentException("Either --ids or both --account and --resource-group must be provided for account update.");
        }
    }

    private static void ValidateUnsupportedUpdateArguments(AccountUpdateOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        if (options.Add is { Length: > 0 } || options.Set is { Length: > 0 } || options.Remove is { Length: > 0 } || options.ForceString)
        {
            throw new ArgumentException("The generic update arguments --add, --set, --remove, and --force-string are not supported by this command yet.");
        }
    }

    public record AccountUpdateCommandResult([property: JsonPropertyName("account")] NetAppAccountCreateResult Account);
}
