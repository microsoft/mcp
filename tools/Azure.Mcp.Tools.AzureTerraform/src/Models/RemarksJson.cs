// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

internal sealed class RemarksJson
{
    [JsonPropertyName("TerraformSamples")]
    public List<TerraformSampleEntry>? TerraformSamples { get; set; }
}
