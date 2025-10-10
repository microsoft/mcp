// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Extension.Commands;
using Azure.Mcp.Tools.Extension.Models;

namespace Azure.Mcp;

[JsonSerializable(typeof(AzureCliGenerateRequest))]
[JsonSerializable(typeof(AzqrReportResult))]
[JsonSerializable(typeof(CliGenerateResult))]
[JsonSerializable(typeof(CliInstallResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(List<string>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class ExtensionJsonContext : JsonSerializerContext
{

}
