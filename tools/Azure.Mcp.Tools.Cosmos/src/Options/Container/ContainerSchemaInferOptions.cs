// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options.Container;

public sealed class ContainerSchemaInferOptions : ISubscriptionOption
{
    [Option(Description = "Number of documents to sample for schema inference (1-20). Defaults to 10.")]
    public int? SampleSize { get; set; }

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
