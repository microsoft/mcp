// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Cosmos.Options.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.Mcp.Tools.Cosmos.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Item;

[CommandMetadata(
    Id = "8b4c5d6e-7f80-4a91-bc23-d4e5f6a7b8c9",
    Name = "text-search",
    Title = "Text Search Cosmos DB Documents",
    Description = "Retrieve the TOP N documents in a Cosmos DB container where a given --search-property matches a provided --search-phrase using the Cosmos FullTextContains function. Matching is word-tokenized (not substring) and uses the container's configured full-text analyzer, so language-specific stemming and stop-word filtering apply (e.g., common English words like 'the' or 'hello' may be excluded from the index). Requires a Cosmos DB full-text index on --search-property. Use --count to control how many documents are returned (1-20, default is 10). Optionally pass --properties-to-select to project specific fields instead of the full document.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ItemTextSearchCommand(ILogger<ItemTextSearchCommand> logger, ICosmosService cosmosService, ISubscriptionResolver subscriptionResolver)
    : BaseCosmosCommand<ItemTextSearchOptions, ItemTextSearchCommand.ItemTextSearchCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ItemTextSearchCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    public override void ValidateOptions(ItemTextSearchOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!PropertyValidator.IsValid(options.SearchProperty))
        {
            validationResult.Errors.Add("--search-property must use dot notation with letters, digits, and underscores only (e.g., name or profile.name).");
        }

        if (!string.IsNullOrWhiteSpace(options.PropertiesToSelect))
        {
            if (options.PropertiesToSelect.Contains('*'))
            {
                validationResult.Errors.Add("--properties-to-select must be a comma-separated list of explicit property names (no '*' wildcards). Omit the option to return all properties.");
            }
            else
            {
                var invalidProperties = options.PropertiesToSelect
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(prop => !PropertyValidator.IsValid(prop))
                    .ToList();

                if (invalidProperties.Count > 0)
                {
                    validationResult.Errors.Add($"--properties-to-select contains invalid property name(s) '{string.Join("', '", invalidProperties)}'. Use letters, digits, and underscores only.");
                }
            }
        }

        if (options.Count != null && (options.Count < 1 || options.Count > 20))
        {
            validationResult.Errors.Add("--count must be between 1 and 20.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemTextSearchOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var propertiesToSelect = string.IsNullOrWhiteSpace(options.PropertiesToSelect)
                ? null
                : options.PropertiesToSelect
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var items = await _cosmosService.TextSearch(
                options.Account,
                options.Database,
                options.Container,
                options.SearchProperty,
                options.SearchPhrase,
                propertiesToSelect,
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
                Name, options.Account, options.Database, options.Container, options.SearchProperty);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ItemTextSearchCommandResult(List<JsonElement> Items);
}
