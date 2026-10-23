// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.FirewallRule;

public sealed class FirewallRuleDeleteOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.FirewallRuleName)]
    public required string FirewallRuleName { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
