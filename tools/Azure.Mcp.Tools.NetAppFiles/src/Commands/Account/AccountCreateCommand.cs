// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    Id = "b8d4e2a6-5c3f-4e7a-9b1d-f6a2c8e3d5b7",
    Name = "create",
    Description =
        """
        Creates an Azure NetApp Files account in a specified resource group and location, and returns the created account details including name, location, resource group, and provisioning state. Requires account name, resource group, location, and subscription.
        """,
    Title = "Create NetApp Files Account",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class AccountCreateCommand(ILogger<AccountCreateCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<AccountCreateOptions, AccountCreateCommand.AccountCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<AccountCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AccountCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
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

            var account = await _netAppFilesService.CreateAccount(
                options.Account!,
                options.ResourceGroup!,
                options.Location!,
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
                new(account),
                NetAppFilesJsonContext.Default.AccountCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating NetApp Files account. Account: {Account}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
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
            $"Authorization failed creating the account. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Resource group not found. Verify it exists and you have access.",
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

    public record AccountCreateCommandResult([property: JsonPropertyName("account")] NetAppAccountCreateResult Account);
}
