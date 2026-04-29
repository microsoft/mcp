// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

[JsonSerializable(typeof(ServiceInfoCommand.ServiceInfoCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull    
)]
internal partial class ServiceInfoJsonContext : JsonSerializerContext;
