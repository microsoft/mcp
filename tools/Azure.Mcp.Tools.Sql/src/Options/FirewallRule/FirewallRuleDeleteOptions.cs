// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Options.FirewallRule;

public class FirewallRuleDeleteOptions : BaseSqlOptions
{
    [JsonPropertyName(SqlOptionDefinitions.FirewallRuleName)]
    public string? FirewallRuleName { get; set; }
}
