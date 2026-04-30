// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Services.ProcessExecution;

namespace Microsoft.Mcp.Core.Services;

[JsonSerializable(typeof(ExternalProcessService.ParseError))]
[JsonSerializable(typeof(ExternalProcessService.ParseOutput))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class ServicesJsonContext : JsonSerializerContext
{

}
