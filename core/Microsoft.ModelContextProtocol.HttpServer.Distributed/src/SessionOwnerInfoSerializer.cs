// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.ModelContextProtocol.HttpServer.Distributed.Abstractions;

namespace Microsoft.ModelContextProtocol.HttpServer.Distributed;

/// <summary>
/// Source-generated JSON serializer for <see cref="SessionOwnerInfo"/>.
/// Uses the generated <see cref="SerializerContext"/> for AOT-compatible,
/// high-performance serialization without reflection.
/// </summary>
internal sealed class SessionOwnerInfoSerializer : IHybridCacheSerializer<SessionOwnerInfo>
{
    /// <summary>
    /// Deserializes a <see cref="SessionOwnerInfo"/> from a buffer using source-generated JSON.
    /// </summary>
    public SessionOwnerInfo Deserialize(ReadOnlySequence<byte> source)
    {
        var reader = new Utf8JsonReader(source);
        return JsonSerializer.Deserialize(ref reader, SerializerContext.Default.SessionOwnerInfo)
            ?? throw new InvalidOperationException("Failed to deserialize SessionOwnerInfo");
    }

    /// <summary>
    /// Serializes a <see cref="SessionOwnerInfo"/> to a buffer using source-generated JSON.
    /// </summary>
    public void Serialize(SessionOwnerInfo value, IBufferWriter<byte> target)
    {
        using Utf8JsonWriter writer = new(target);
        JsonSerializer.Serialize(writer, value, SerializerContext.Default.SessionOwnerInfo);
    }
}
