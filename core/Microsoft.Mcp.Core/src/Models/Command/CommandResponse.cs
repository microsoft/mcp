// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Mcp.Core.Models.Command;

public class CommandResponse
{
    [JsonPropertyName("status")]
    [JsonConverter(typeof(HttpStatusCodeConverter))]
    public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseResult? Results { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    /// <summary>
    /// Optional images to include as MCP image content blocks alongside the JSON response.
    /// When populated, each image is emitted as an <c>ImageContentBlock</c> in the MCP
    /// <c>CallToolResult</c>, enabling vision-capable LLM clients to consume charts or
    /// visualizations directly without parsing raw data.
    /// </summary>
    [JsonPropertyName("images")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ResponseImage>? Images { get; set; }
}

/// <summary>
/// Represents an image to be included in an MCP tool response as an image content block.
/// </summary>
/// <param name="Data">The raw image bytes.</param>
/// <param name="MimeType">The MIME type of the image (e.g. <c>image/png</c>).</param>
/// <param name="AltText">Optional human-readable description of the image for accessibility and fallback.</param>
public sealed record ResponseImage(
    [property: JsonPropertyName("data")] byte[] Data,
    [property: JsonPropertyName("mimeType")] string MimeType,
    [property: JsonPropertyName("altText")] string? AltText = null);

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

public class ResultConverter : JsonConverter<ResponseResult>
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

public class HttpStatusCodeConverter : JsonConverter<HttpStatusCode>
{
    public override HttpStatusCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return (HttpStatusCode)reader.GetInt32();
        }
        throw new JsonException("Expected a number for HttpStatusCode.");
    }

    public override void Write(Utf8JsonWriter writer, HttpStatusCode value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}
