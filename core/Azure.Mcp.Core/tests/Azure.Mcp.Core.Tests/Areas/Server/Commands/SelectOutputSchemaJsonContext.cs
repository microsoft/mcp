// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands;

[JsonSerializable(typeof(SelectOutputSchemaSample))]
[JsonSerializable(typeof(SelectOutputSchemaSample[]))]
[JsonSerializable(typeof(SelectOutputSchemaCollection))]
[JsonSerializable(typeof(RecursiveSelectOutputSchemaSample))]
[JsonSerializable(typeof(RecursiveSelectOutputSchemaSample[]))]
[JsonSerializable(typeof(RecursiveSelectOutputSchemaCollection))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SelectOutputSchemaJsonContext : JsonSerializerContext;
