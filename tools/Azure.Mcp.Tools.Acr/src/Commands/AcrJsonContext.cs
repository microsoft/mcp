// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Acr.Commands.Registry;
using Azure.Mcp.Tools.Acr.Services.Models;

namespace Azure.Mcp.Tools.Acr.Commands;

[JsonSerializable(typeof(RegistryListCommand.RegistryListCommandResult))]
[JsonSerializable(typeof(RegistryRepositoryListCommand.RegistryRepositoryListCommandResult))]
[JsonSerializable(typeof(Models.AcrRegistryInfo))]
[JsonSerializable(typeof(ContainerRegistryData))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class AcrJsonContext : JsonSerializerContext
{
}
