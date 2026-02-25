// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureMigrate.Models;

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

[JsonSerializable(typeof(MigrateProjectCreateContent))]
[JsonSerializable(typeof(MigrateProjectProperties))]
[JsonSerializable(typeof(MigrateProjectResult))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AzureMigrateSerializerContext : JsonSerializerContext;
