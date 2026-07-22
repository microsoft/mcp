// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public sealed class ItemTextSearchOptions : ISubscriptionOption
{
    [Option(Description = "The document property to search. Supports dot notation (e.g., 'name' or 'profile.name'). Allowed characters: letters, digits, and underscores.")]
    public required string SearchProperty { get; set; }

    [Option(Description = "The phrase to search for. Passed as a parameterized value to a Cosmos DB FullTextContains query. The container must have a full-text index on the property.")]
    public required string SearchPhrase { get; set; }

    [Option(Description = CosmosOptionDescriptions.Count)]
    public int? Count { get; set; }

    [Option(Description = CosmosOptionDescriptions.PropertiesToSelect)]
    public string? PropertiesToSelect { get; set; }

    [Option(Description = CosmosOptionDescriptions.Container)]
    public required string Container { get; set; }

    [Option(Description = CosmosOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(Description = CosmosOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(Description = OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }
}
