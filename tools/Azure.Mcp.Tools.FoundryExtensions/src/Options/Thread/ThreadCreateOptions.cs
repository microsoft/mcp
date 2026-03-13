// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Thread;

public class ThreadCreateOptions : GlobalOptions
{
    [JsonPropertyName(FoundryExtensionsOptionDefinitions.Endpoint)]
    public string? Endpoint { get; set; }

    [JsonPropertyName(FoundryExtensionsOptionDefinitions.UserMessage)]
    public string? UserMessage { get; set; }
}
