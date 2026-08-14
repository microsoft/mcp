// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.MySql.Options;

/// <summary>
/// Options for MySQL commands that need server access.
/// </summary>
public class MySqlServerOptions : ISubscriptionOption
{
    [Option(Description = "The MySQL server to be accessed.")]
    public required string Server { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}

/// <summary>
/// Options for MySQL commands that need database access.
/// </summary>
public class MySqlDatabaseOptions : MySqlServerOptions
{
    [Option(Description = "The MySQL database to be accessed.")]
    public required string Database { get; set; }

    [Option(Description = MySqlOptionDescriptions.User)]
    public required string User { get; set; }
}
