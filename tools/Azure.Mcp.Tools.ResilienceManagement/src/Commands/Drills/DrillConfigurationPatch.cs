// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.ResourceManager.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

// SDK 1.0.0 reuses create models inside PATCH and writes omitted required-on-create fields as null/empty.
// Its serialization extension point preserves the validated sparse GA configuration without changing transport or LRO handling.
internal sealed class DrillConfigurationPatch(JsonElement configuration) : DrillUpdateProperties
{
    private readonly JsonElement _configuration = configuration.Clone();

    protected override void JsonModelWriteCore(Utf8JsonWriter writer, ModelReaderWriterOptions options)
    {
        base.JsonModelWriteCore(writer, options);
        foreach (JsonProperty property in _configuration.EnumerateObject())
        {
            property.WriteTo(writer);
        }
    }
}
