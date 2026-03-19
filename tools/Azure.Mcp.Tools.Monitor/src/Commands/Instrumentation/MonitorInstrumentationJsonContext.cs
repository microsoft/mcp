// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Monitor.Commands;

namespace Azure.Mcp.Tools.Monitor.Commands;

[JsonSerializable(typeof(GetLearningResourceCommand.GetLearningResourceCommandResult))]
[JsonSerializable(typeof(string))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class MonitorInstrumentationJsonContext : JsonSerializerContext;
