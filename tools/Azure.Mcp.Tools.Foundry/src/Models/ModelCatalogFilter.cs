// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Models;

public class ModelCatalogFilter(string field, string[] values, string @operator)
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = field;

    [JsonPropertyName("values")]
    public string[] Values { get; set; } = values;

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = @operator;
}
