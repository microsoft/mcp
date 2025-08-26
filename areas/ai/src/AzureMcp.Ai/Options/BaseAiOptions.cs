// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using AzureMcp.Core.Options;

namespace AzureMcp.Ai.Options;

public class BaseAiOptions : SubscriptionOptions
{
    [JsonPropertyName(AiOptionDefinitions.ResourceNameName)]
    public string? ResourceName { get; set; }
}
