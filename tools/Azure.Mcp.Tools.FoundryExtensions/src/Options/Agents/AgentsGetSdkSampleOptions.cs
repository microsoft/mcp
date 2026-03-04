// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Agents;

public class AgentsGetSdkSampleOptions
{
    [JsonPropertyName(FoundryExtensionsOptionDefinitions.ProgrammingLanguage)]
    public string? ProgrammingLanguage { get; set; }
}
