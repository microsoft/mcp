// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.ProcessExecution;

namespace Azure.Mcp;

[JsonSerializable(typeof(ExternalProcessService.ParseError))]
[JsonSerializable(typeof(ExternalProcessService.ParseOutput))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class ServicesJsonContext : JsonSerializerContext
{

}
