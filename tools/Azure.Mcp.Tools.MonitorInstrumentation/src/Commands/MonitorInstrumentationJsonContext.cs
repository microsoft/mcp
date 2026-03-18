// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.MonitorInstrumentation.Commands;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(GetLearningResourceCommand.GetLearningResourceCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class MonitorInstrumentationJsonContext : JsonSerializerContext;
