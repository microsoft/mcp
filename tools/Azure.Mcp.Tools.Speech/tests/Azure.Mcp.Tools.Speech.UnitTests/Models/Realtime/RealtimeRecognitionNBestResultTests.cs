// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models.Realtime;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class RealtimeRecognitionNBestResultTests
{
    [Fact]
    public void RealtimeRecognitionNBestResult_DefaultValues_ShouldBeNull()
    {
        // Arrange & Act
        var result = new RealtimeRecognitionNBestResult();

        // Assert
        Assert.Null(result.Display);

        Assert.Null(result.Words);
    }

    [Fact]
    public void RealtimeRecognitionNBestResult_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var words = new List<RealtimeRecognitionWordResult>
        {
            new() { Word = "Hello",  },
            new() { Word = "world",  }
        };

        // Act
        var result = new RealtimeRecognitionNBestResult
        {
            Display = "Hello world",

            Words = words
        };

        // Assert
        Assert.Equal("Hello world", result.Display);

        Assert.NotNull(result.Words);
        Assert.Equal(2, result.Words.Count);
        Assert.Equal("Hello", result.Words[0].Word);
    }

    [Fact]
    public void RealtimeRecognitionNBestResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = new RealtimeRecognitionNBestResult
        {
            Display = "Hello world",

            Words = new List<RealtimeRecognitionWordResult>
            {
                new() { Word = "Hello",  Offset = 100, Duration = 500 },
                new() { Word = "world",  Offset = 600, Duration = 400 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"display\":\"Hello world\"", json);
        Assert.Contains("\"words\":", json);
        Assert.Contains("\"Hello\"", json);
        Assert.Contains("\"world\"", json);
    }

    [Fact]
    public void RealtimeRecognitionNBestResult_JsonDeserialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "display": "Hello world",
            "words": [
                {
                    "word": "Hello",
                    "offset": 100,
                    "duration": 500
                },
                {
                    "word": "world",
                    "offset": 600,
                    "duration": 400
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<RealtimeRecognitionNBestResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello world", result.Display);

        Assert.NotNull(result.Words);
        Assert.Equal(2, result.Words.Count);
        Assert.Equal("Hello", result.Words[0].Word);
        Assert.Equal((ulong)100, result.Words[0].Offset);
        Assert.Equal((ulong)500, result.Words[0].Duration);
    }
}
