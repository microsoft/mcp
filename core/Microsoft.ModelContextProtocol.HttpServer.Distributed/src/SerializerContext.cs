// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.ModelContextProtocol.HttpServer.Distributed.Abstractions;

namespace Microsoft.ModelContextProtocol.HttpServer.Distributed;

/// <summary>
/// JSON serialization context for distributed session store.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(SessionOwnerInfo))]
internal sealed partial class SerializerContext : JsonSerializerContext { }
