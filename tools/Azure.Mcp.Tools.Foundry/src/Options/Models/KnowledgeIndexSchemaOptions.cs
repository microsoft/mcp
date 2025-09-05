// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Foundry.Options.Models;

public class KnowledgeIndexSchemaOptions : BaseKnowledgeIndexOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.IndexName)]
    public string? IndexName { get; set; }
}
