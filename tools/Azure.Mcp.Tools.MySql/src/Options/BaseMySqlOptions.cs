// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.MySql.Options;

/// <summary>
/// Base options for MySQL commands that only need subscription, resource group, and user.
/// </summary>
public class BaseMySqlOptions : SubscriptionOptions
{
    [JsonPropertyName(MySqlOptionDefinitions.UserName)]
    public string? User { get; set; }
}

/// <summary>
/// Options for MySQL commands that need server access.
/// </summary>
public class MySqlServerOptions : BaseMySqlOptions
{
    [JsonPropertyName(MySqlOptionDefinitions.ServerName)]
    public string? Server { get; set; }
}

/// <summary>
/// Options for MySQL commands that need database access.
/// </summary>
public class MySqlDatabaseOptions : MySqlServerOptions
{
    [JsonPropertyName(MySqlOptionDefinitions.DatabaseName)]
    public string? Database { get; set; }
}
