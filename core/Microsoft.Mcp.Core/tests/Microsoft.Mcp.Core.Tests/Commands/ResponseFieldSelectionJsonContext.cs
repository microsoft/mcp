// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Tests.Commands;

[JsonSerializable(typeof(List<ExceptionResult>))]
[JsonSerializable(typeof(string))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ResponseFieldSelectionJsonContext : JsonSerializerContext;
