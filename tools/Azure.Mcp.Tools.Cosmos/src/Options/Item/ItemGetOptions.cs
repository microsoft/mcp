// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public sealed class ItemGetOptions : ISubscriptionOption
{
    [Option(Description = "The id of the document to retrieve.")]
    public required string Id { get; set; }

    [Option(Description = "Optional partition key value for the document. When provided, the query is scoped to a single partition (cheaper than a cross-partition fan-out).")]
    public string? PartitionKey { get; set; }

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
