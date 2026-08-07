// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
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
public sealed class AccountCreateCommand(ILogger<AccountCreateCommand> logger, INetAppFilesService netAppFilesService) : SubscriptionCommand<AccountCreateOptions>()
{
    private readonly ILogger<AccountCreateCommand> _logger = logger;

    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(NetAppFilesOptionDefinitions.Location);
        command.Options.Add(NetAppFilesOptionDefinitions.Tags.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.KeyName.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.KeySource.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.KeyVaultResourceId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.KeyVaultUri.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.FederatedClientId.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.UserAssignedIdentity.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.IdentityType.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.UserAssignedIdentities.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ActiveDirectories.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.NfsV4IdDomain.AsOptional());
    }

    protected override AccountCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Location = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Location.Name);
        options.Tags = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Tags.Name);
        options.KeyName = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.KeyName.Name);
        options.KeySource = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.KeySource.Name);
        options.KeyVaultResourceId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.KeyVaultResourceId.Name);
        options.KeyVaultUri = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.KeyVaultUri.Name);
        options.FederatedClientId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.FederatedClientId.Name);
        options.UserAssignedIdentity = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.UserAssignedIdentity.Name);
        options.IdentityType = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.IdentityType.Name);
        options.UserAssignedIdentities = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.UserAssignedIdentities.Name);
        options.ActiveDirectories = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ActiveDirectories.Name);
        options.NfsV4IdDomain = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.NfsV4IdDomain.Name);
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

    internal record AccountCreateCommandResult([property: JsonPropertyName("account")] NetAppAccountCreateResult Account);
}
