// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;

[JsonSerializable(typeof(List<string>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class AzureTerraformBestPracticesJsonContext : JsonSerializerContext
{

}
