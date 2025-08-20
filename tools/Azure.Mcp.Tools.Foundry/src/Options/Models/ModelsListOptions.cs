// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Models;

public class ModelsListOptions : GlobalOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.SearchForFreePlayground)]
    public bool? SearchForFreePlayground { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.PublisherName)]
    public string? PublisherName { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.LicenseName)]
    public string? LicenseName { get; set; }
    [JsonPropertyName(FoundryOptionDefinitions.ModelName)]
    public string? ModelName { get; set; }
}
