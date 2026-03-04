// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FoundryExtensions.Options.Models;

public class ResourceGetOptions : SubscriptionOptions
{
    [JsonPropertyName(FoundryExtensionsOptionDefinitions.ResourceName)]
    public string? ResourceName { get; set; }
}
