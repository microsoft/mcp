// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models.Realtime;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class RealtimeRecognitionResultTests
{
    [Fact]
    public void RealtimeRecognitionResult_DefaultValues_ShouldBeNull()
    {
        // Arrange & Act
        var result = new RealtimeRecognitionResult();

        // Assert
        Assert.Null(result.Text);

        Assert.Null(result.Offset);
        Assert.Null(result.Duration);
        Assert.Null(result.Language);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void RealtimeRecognitionResult_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var result = new RealtimeRecognitionResult();

        // Act
        result.Text = "Hello world";
        result.Offset = 1000;
        result.Duration = 2000;
        result.Language = "en-US";
        result.Reason = "RecognizedSpeech";

        // Assert
        Assert.Equal("Hello world", result.Text);
        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)2000, result.Duration);
        Assert.Equal("en-US", result.Language);
        Assert.Equal("RecognizedSpeech", result.Reason);
    }

    [Fact]
    public void RealtimeRecognitionResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = new RealtimeRecognitionResult
        {
            Text = "Hello world",

            Offset = 1000,
            Duration = 2000,
            Language = "en-US",
            Reason = "RecognizedSpeech"
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"text\":\"Hello world\"", json);
        Assert.Contains("\"offset\":1000", json);
        Assert.Contains("\"duration\":2000", json);
        Assert.Contains("\"language\":\"en-US\"", json);
        Assert.Contains("\"reason\":\"RecognizedSpeech\"", json);
    }

    [Fact]
    public void RealtimeRecognitionResult_JsonDeserialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "text": "Hello world",
            "offset": 1000,
            "duration": 2000,
            "language": "en-US",
            "reason": "RecognizedSpeech"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<RealtimeRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello world", result.Text);

        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)2000, result.Duration);
        Assert.Equal("en-US", result.Language);
        Assert.Equal("RecognizedSpeech", result.Reason);
    }

    [Fact]
    public void RealtimeRecognitionResult_JsonDeserialization_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var json = """
        {
            "text": null,
            "offset": null,
            "duration": null,
            "language": null,
            "reason": null
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<RealtimeRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Text);

        Assert.Null(result.Offset);
        Assert.Null(result.Duration);
        Assert.Null(result.Language);
        Assert.Null(result.Reason);
    }
}
