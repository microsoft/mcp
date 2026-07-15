// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.FirewallRule;

public sealed class FirewallRuleCreateOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.FirewallRuleName)]
    public required string FirewallRuleName { get; set; }

    [Option(Description = "The start IP address of the firewall rule range.")]
    public required string StartIpAddress { get; set; }

    [Option(Description = "The end IP address of the firewall rule range.")]
    public required string EndIpAddress { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
