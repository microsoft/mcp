// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Services.Models;

/// <summary>
/// A class representing the SqlFirewallRule properties data model.
/// A server firewall rule properties.
/// </summary>
internal sealed class SqlFirewallRuleProperties
{
    /// <summary> The start IP address of the firewall rule. Must be IPv4 format. Use value '0.0.0.0' for all Azure-internal IP addresses. </summary>
    [JsonPropertyName("startIpAddress")]
    public string? StartIPAddress { get; set; }
    /// <summary> The end IP address of the firewall rule. Must be IPv4 format. Must be greater than or equal to startIpAddress. Use value '0.0.0.0' for all Azure-internal IP addresses. </summary>
    [JsonPropertyName("endIpAddress")]
    public string? EndIPAddress { get; set; }
}
