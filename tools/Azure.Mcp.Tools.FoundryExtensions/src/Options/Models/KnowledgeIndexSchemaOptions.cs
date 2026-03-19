// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public class KnowledgeIndexSchemaOptions : BaseKnowledgeIndexOptions
{
    [JsonPropertyName(FoundryExtensionsOptionDefinitions.IndexName)]
    public string? IndexName { get; set; }
}
