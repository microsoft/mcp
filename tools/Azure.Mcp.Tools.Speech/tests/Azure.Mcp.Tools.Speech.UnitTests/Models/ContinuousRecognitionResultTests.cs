// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class ContinuousRecognitionResultTests
{
    [Fact]
    public void ContinuousRecognitionResult_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var result = new ContinuousRecognitionResult();

        // Assert
        Assert.Null(result.FullText);
        Assert.NotNull(result.Segments);
        Assert.Empty(result.Segments);
    }

    [Fact]
    public void ContinuousRecognitionResult_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var segments = new List<SpeechRecognitionResult>
        {
            new() { Text = "Hello", Offset = 0, Duration = 1000 },
            new() { Text = "world", Offset = 1000, Duration = 1500 }
        };

        // Act
        var result = new ContinuousRecognitionResult
        {
            FullText = "Hello world",
            Segments = segments
        };

        // Assert
        Assert.Equal("Hello world", result.FullText);
        Assert.NotNull(result.Segments);
        Assert.Equal(2, result.Segments.Count);
        Assert.Equal("Hello", result.Segments[0].Text);
        Assert.Equal("world", result.Segments[1].Text);
    }

    [Fact]
    public void ContinuousRecognitionResult_WithEmptySegments_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var result = new ContinuousRecognitionResult
        {
            FullText = "Complete text",
            Segments = new List<SpeechRecognitionResult>()
        };

        // Assert
        Assert.Equal("Complete text", result.FullText);
        Assert.NotNull(result.Segments);
        Assert.Empty(result.Segments);
    }

    [Fact]
    public void ContinuousRecognitionResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = new ContinuousRecognitionResult
        {
            FullText = "Hello world test",
            Segments = new List<SpeechRecognitionResult>
            {
                new() { Text = "Hello", Offset = 0, Duration = 1000, Language = "en-US" },
                new() { Text = "world", Offset = 1000, Duration = 1200, Language = "en-US" },
                new() { Text = "test", Offset = 2200, Duration = 800, Language = "en-US" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"fullText\":\"Hello world test\"", json);
        Assert.Contains("\"segments\":", json);
        Assert.Contains("\"Hello\"", json);
        Assert.Contains("\"world\"", json);
        Assert.Contains("\"test\"", json);
    }

    [Fact]
    public void ContinuousRecognitionResult_JsonDeserialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "fullText": "Hello world test",
            "segments": [
                {
                    "text": "Hello",
                    "offset": 0,
                    "duration": 1000,
                    "language": "en-US",
                    "reason": "RecognizedSpeech"
                },
                {
                    "text": "world",
                    "offset": 1000,
                    "duration": 1200,
                    "language": "en-US",
                    "reason": "RecognizedSpeech"
                },
                {
                    "text": "test",
                    "offset": 2200,
                    "duration": 800,
                    "language": "en-US",
                    "reason": "RecognizedSpeech"
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ContinuousRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello world test", result.FullText);
        Assert.NotNull(result.Segments);
        Assert.Equal(3, result.Segments.Count);

        Assert.Equal("Hello", result.Segments[0].Text);
        Assert.Equal((ulong)0, result.Segments[0].Offset);
        Assert.Equal((ulong)1000, result.Segments[0].Duration);
        Assert.Equal("en-US", result.Segments[0].Language);

        Assert.Equal("world", result.Segments[1].Text);
        Assert.Equal((ulong)1000, result.Segments[1].Offset);
        Assert.Equal((ulong)1200, result.Segments[1].Duration);

        Assert.Equal("test", result.Segments[2].Text);
        Assert.Equal((ulong)2200, result.Segments[2].Offset);
        Assert.Equal((ulong)800, result.Segments[2].Duration);
    }

    [Fact]
    public void ContinuousRecognitionResult_JsonDeserialization_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var json = """
        {
            "fullText": null,
            "segments": []
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ContinuousRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.FullText);
        Assert.NotNull(result.Segments);
        Assert.Empty(result.Segments);
    }

    [Fact]
    public void ContinuousRecognitionResult_JsonDeserialization_WithMissingSegments_ShouldUseDefault()
    {
        // Arrange
        var json = """
        {
            "fullText": "Complete text only"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<ContinuousRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Complete text only", result.FullText);
        Assert.NotNull(result.Segments);
        Assert.Empty(result.Segments); // Default empty list from model initialization
    }

    [Fact]
    public void ContinuousRecognitionResult_WithSingleSegment_ShouldWorkCorrectly()
    {
        // Arrange
        var segment = new SpeechRecognitionResult
        {
            Text = "Single segment",
            Offset = 500,
            Duration = 2000,
            Language = "en-US",
            Reason = "RecognizedSpeech"
        };

        // Act
        var result = new ContinuousRecognitionResult
        {
            FullText = "Single segment",
            Segments = new List<SpeechRecognitionResult> { segment }
        };

        // Assert
        Assert.Equal("Single segment", result.FullText);
        Assert.Single(result.Segments);
        Assert.Equal("Single segment", result.Segments[0].Text);
        Assert.Equal((ulong)500, result.Segments[0].Offset);
        Assert.Equal((ulong)2000, result.Segments[0].Duration);
        Assert.Equal("en-US", result.Segments[0].Language);
        Assert.Equal("RecognizedSpeech", result.Segments[0].Reason);
    }
}
