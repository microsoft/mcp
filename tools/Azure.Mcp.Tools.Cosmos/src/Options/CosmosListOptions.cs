// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class CosmosListOptions : ISubscriptionOption
{
    [Option("The name of the database (optional). Requires --account to be specified. When provided, lists containers within this database.")]
    public string? Database { get; set; }

    [Option("The name of the Cosmos DB account (optional). When not specified, lists all accounts in the subscription. Specify this to list databases, or combine with --database to list containers.")]
    public string? Account { get; set; }

    [Option(OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }
}
