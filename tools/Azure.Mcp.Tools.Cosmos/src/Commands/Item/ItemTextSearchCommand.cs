// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Options.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.Mcp.Tools.Cosmos.Validation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Item;

[CommandMetadata(
    Id = "8b4c5d6e-7f80-4a91-bc23-d4e5f6a7b8c9",
    Name = "text-search",
    Title = "Text Search Cosmos DB Documents",
    Description = "Retrieve the TOP N documents in a Cosmos DB container where the specified property contains the provided search string. Use the --count option to control how many documents are returned (1-20, default is 10). Requires a Cosmos DB full-text index on the property.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ItemTextSearchCommand(ILogger<ItemTextSearchCommand> logger, ICosmosService cosmosService)
    : BaseContainerCommand<ItemTextSearchOptions>()
{
    private readonly ILogger<ItemTextSearchCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.Property);
        command.Options.Add(CosmosOptionDefinitions.SearchPhrase);
        command.Options.Add(CosmosOptionDefinitions.Count);
        command.Validators.Add(result =>
        {
            var property = result.GetValueOrDefault<string>(CosmosOptionDefinitions.Property.Name);
            if (!PropertyValidator.IsValid(property))
            {
                result.AddError("--property must use dot notation with letters, digits, and underscores only (e.g., name or profile.name).");
            }

            var count = result.GetValueOrDefault<int>(CosmosOptionDefinitions.Count.Name);
            if (count < 1 || count > 20)
            {
                result.AddError("--count must be between 1 and 20.");
            }
        });
    }

    protected override ItemTextSearchOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Property = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.Property.Name);
        options.SearchPhrase = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.SearchPhrase.Name);
        options.Count = parseResult.GetValueOrDefault<int>(CosmosOptionDefinitions.Count.Name);
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
            var items = await _cosmosService.TextSearch(
                options.Account!,
                options.Database!,
                options.Container!,
                options.Property!,
                options.SearchPhrase!,
                options.Count ?? 10,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ItemTextSearchCommandResult(items ?? []),
                CosmosJsonContext.Default.ItemTextSearchCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Account: {Account}, Database: {Database}, Container: {Container}, Property: {Property}",
                Name, options.Account, options.Database, options.Container, options.Property);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.StatusCode,
        _ => base.GetStatusCode(ex)
    };

    internal record ItemTextSearchCommandResult(List<JsonElement> Items);
}
