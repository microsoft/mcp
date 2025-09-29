// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class NBestResultTests
{
    [Fact]
    public void NBestResult_DefaultValues_ShouldBeNull()
    {
        // Arrange & Act
        var result = new NBestResult();

        // Assert
        Assert.Null(result.Display);

        Assert.Null(result.Words);
    }

    [Fact]
    public void NBestResult_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var words = new List<WordResult>
        {
            new() { Word = "Hello",  },
            new() { Word = "world",  }
        };

        // Act
        var result = new NBestResult
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
    public void NBestResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = new NBestResult
        {
            Display = "Hello world",

            Words = new List<WordResult>
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
    public void NBestResult_JsonDeserialization_ShouldDeserializeCorrectly()
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
        var result = JsonSerializer.Deserialize<NBestResult>(json);

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
