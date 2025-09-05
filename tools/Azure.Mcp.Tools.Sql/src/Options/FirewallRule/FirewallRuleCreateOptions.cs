// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Options.FirewallRule;

public class FirewallRuleCreateOptions : BaseSqlOptions
{
    [JsonPropertyName(SqlOptionDefinitions.FirewallRuleName)]
    public string? FirewallRuleName { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.StartIpAddress)]
    public string? StartIpAddress { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.EndIpAddress)]
    public string? EndIpAddress { get; set; }
}
