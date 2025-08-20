// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Acr.Commands.Registry;

namespace Azure.Mcp.Tools.Acr.Commands;

[JsonSerializable(typeof(RegistryListCommand.RegistryListCommandResult))]
[JsonSerializable(typeof(Models.AcrRegistryInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AcrJsonContext : JsonSerializerContext
{
}
