// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public class ItemTextSearchOptions : ISubscriptionOption
{
    [Option("The document property to search. Supports dot notation (e.g., 'name' or 'profile.name'). Allowed characters: letters, digits, and underscores.")]
    public required string SearchProperty { get; set; }

    [Option("The phrase to search for. Passed as a parameterized value to a Cosmos DB FullTextContains query. The container must have a full-text index on the property.")]
    public required string SearchPhrase { get; set; }

    [Option(CosmosOptionDescriptions.Count)]
    public int? Count { get; set; }

    [Option(CosmosOptionDescriptions.PropertiesToSelect)]
    public string? PropertiesToSelect { get; set; }

    [Option(CosmosOptionDescriptions.Container)]
    public required string Container { get; set; }

    [Option(CosmosOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(CosmosOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }
}
