// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.MySql.Options;

public sealed class MySqlListOptions : ISubscriptionOption
{
    [Option(Description = MySqlOptionDescriptions.User)]
    public string? User { get; set; }

    [Option(Description = "The MySQL server to list databases from (requires --user).")]
    public string? Server { get; set; }

    [Option(Description = "The MySQL database to list tables from (requires --server).")]
    public string? Database { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }
}
