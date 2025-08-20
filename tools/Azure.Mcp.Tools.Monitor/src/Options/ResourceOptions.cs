// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Options
{
    public abstract class ResourceOptions : BaseMonitorOptions, IResourceOptions
    {
        [JsonPropertyName(ResourceLogQueryOptionDefinitions.ResourceIdName)]
        public string? ResourceId { get; set; }
    }
}
