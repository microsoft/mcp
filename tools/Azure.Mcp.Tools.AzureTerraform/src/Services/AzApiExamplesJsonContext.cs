// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

[JsonSerializable(typeof(RemarksJson))]
[JsonSerializable(typeof(List<TerraformSampleEntry>))]
internal sealed partial class AzApiExamplesJsonContext : JsonSerializerContext;
