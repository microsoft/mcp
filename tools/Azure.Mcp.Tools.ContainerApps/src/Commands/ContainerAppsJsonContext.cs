// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ContainerApps.Commands.ContainerApp;

namespace Azure.Mcp.Tools.ContainerApps.Commands;

[JsonSerializable(typeof(ContainerAppListCommand.ContainerAppListCommandResult))]
[JsonSerializable(typeof(Models.ContainerAppInfo))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class ContainerAppsJsonContext : JsonSerializerContext
{
}
