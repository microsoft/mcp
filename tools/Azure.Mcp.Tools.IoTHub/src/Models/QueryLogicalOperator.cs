// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

[JsonConverter(typeof(JsonStringEnumConverter<QueryLogicalOperator>))]
public enum QueryLogicalOperator
{
    [JsonStringEnumMemberName("AND")]
    And,

    [JsonStringEnumMemberName("OR")]
    Or
}
