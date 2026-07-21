// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureTerraform.Models;

internal sealed record RemarksJson([property: JsonPropertyName("TerraformSamples")] List<TerraformSampleEntry> TerraformSamples);
