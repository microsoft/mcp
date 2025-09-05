// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Sql.Commands;

namespace Azure.Mcp.Tools.Sql.Services.Models
{
    /// <summary>
    /// A class representing the SqlFirewallRule data model.
    /// A server firewall rule.
    /// </summary>
    internal sealed class SqlFirewallRuleData
    {
        /// <summary> The resource ID for the resource. </summary>
        [JsonPropertyName("id")]
        public string? ResourceId { get; set; }
        /// <summary> The type of the resource. </summary>
        [JsonPropertyName("type")]
        public string? ResourceType { get; set; }
        /// <summary> The name of the resource. </summary>
        [JsonPropertyName("name")]
        public string? ResourceName { get; set; }
        /// <summary> The properties of the firewall rule. </summary>
        public SqlFirewallRuleProperties? Properties { get; set; }

        // Read the JSON response content and create a model instance from it.
        public static SqlFirewallRuleData? FromJson(JsonElement source)
        {
            return JsonSerializer.Deserialize(source, SqlJsonContext.Default.SqlFirewallRuleData);
        }
    }
}
