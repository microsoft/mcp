// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Azure.Mcp.Core.Models.Command;

public class CommandResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseResult? Results { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}

[JsonConverter(typeof(ResultConverter))]
public sealed class ResponseResult
{
    private readonly object? _result;
    private readonly JsonTypeInfo _typeInfo;

    private ResponseResult(object? result, JsonTypeInfo typeInfo)
    {
        _result = result;
        _typeInfo = typeInfo;
    }

    public static ResponseResult Create<T>(T result, JsonTypeInfo<T> typeInfo)
    {
        return new ResponseResult(result, typeInfo);
    }

    public void Write(Utf8JsonWriter writer)
    {
        JsonSerializer.Serialize(writer, _result, _typeInfo);
    }
}

internal class ResultConverter : JsonConverter<ResponseResult>
{
    public override ResponseResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Can't deserialize an object without knowing its type.
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, ResponseResult? value, JsonSerializerOptions options)
    {
        value?.Write(writer);
    }
}
