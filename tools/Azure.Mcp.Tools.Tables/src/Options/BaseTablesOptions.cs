// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Tables.Options;

public class BaseTablesOptions : SubscriptionOptions
{
    public string? StorageAccount { get; set; }
    public string? CosmosDbAccount { get; set; }
}
