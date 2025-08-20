// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.BicepSchema.Options
{
    public class BaseBicepSchemaOptions : SubscriptionOptions
    {
        [JsonPropertyName(BicepSchemaOptionDefinitions.ResourceType)]
        public string? ResourceType { get; set; }
    }
}
