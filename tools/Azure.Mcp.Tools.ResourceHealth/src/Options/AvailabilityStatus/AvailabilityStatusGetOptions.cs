// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResourceHealth.Options.AvailabilityStatus;

public class AvailabilityStatusGetOptions : BaseResourceHealthOptions
{
    [JsonPropertyName(ResourceHealthOptionDefinitions.ResourceIdName)]
    public string? ResourceId { get; set; }
}
