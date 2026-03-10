// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public abstract class BaseKnowledgeIndexOptions : GlobalOptions
{
    [JsonPropertyName(FoundryExtensionsOptionDefinitions.Endpoint)]
    public string? Endpoint { get; set; }
}
