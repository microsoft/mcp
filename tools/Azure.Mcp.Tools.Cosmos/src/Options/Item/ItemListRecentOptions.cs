// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public class ItemListRecentOptions : ISubscriptionOption
{
    [Option(CosmosOptionDescriptions.Count)]
    public int? Count { get; set; }

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
