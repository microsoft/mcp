// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Foundry.Options.Models;

public class ResourceGetOptions : SubscriptionOptions
{
    [JsonPropertyName(FoundryOptionDefinitions.ResourceName)]
    public string? ResourceName { get; set; }
}
