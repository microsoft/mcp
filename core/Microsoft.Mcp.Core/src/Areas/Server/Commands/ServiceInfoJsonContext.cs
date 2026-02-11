// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Server.Commands;

[JsonSerializable(typeof(ServiceInfoCommand.ServiceInfoCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class ServiceInfoJsonContext : JsonSerializerContext
{
}
