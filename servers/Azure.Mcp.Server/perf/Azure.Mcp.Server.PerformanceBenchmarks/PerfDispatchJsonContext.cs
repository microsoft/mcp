// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Server.Perf.Dispatch;

/// <summary>
/// Source-generated JSON context for the no-op benchmark command's result type, so the
/// dispatch benchmarks can serialize it AOT-safely with the same pattern real commands use.
/// </summary>
[JsonSerializable(typeof(NoOpCommand.NoOpResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class PerfDispatchJsonContext : JsonSerializerContext;
