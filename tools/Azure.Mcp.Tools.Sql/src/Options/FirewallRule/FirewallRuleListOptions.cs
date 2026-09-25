// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.FirewallRule;

/// <summary>
/// Options for the SQL Server Firewall Rules List command.
/// </summary>
public sealed class FirewallRuleListOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
