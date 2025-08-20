// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options;

public class BaseSqlOptions : SubscriptionOptions
{
    [JsonPropertyName(SqlOptionDefinitions.ServerName)]
    public string? Server { get; set; }
}
