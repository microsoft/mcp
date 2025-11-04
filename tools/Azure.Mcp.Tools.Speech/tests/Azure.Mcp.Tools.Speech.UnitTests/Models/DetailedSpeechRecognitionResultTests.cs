// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class DetailedSpeechRecognitionResultTests
{
    [Fact]
    public void DetailedSpeechRecognitionResult_InheritsFromSpeechRecognitionResult()
    {
        // Arrange & Act
        var result = new DetailedSpeechRecognitionResult();

        // Assert
        Assert.IsAssignableFrom<SpeechRecognitionResult>(result);
        Assert.Null(result.NBest);
    }

    [Fact]
    public void DetailedSpeechRecognitionResult_WithNBestResults_ShouldRetainValues()
    {
        // Arrange
        var nbestResults = new List<NBestResult>
        {
            new() { Display = "Hello world",  },
            new() { Display = "Hello word",  }
        };

        // Act
        var result = new DetailedSpeechRecognitionResult
        {
            Text = "Hello world",

            NBest = nbestResults
        };

        // Assert
        Assert.Equal("Hello world", result.Text);

        Assert.NotNull(result.NBest);
        Assert.Equal(2, result.NBest.Count);
        Assert.Equal("Hello world", result.NBest[0].Display);
    }

    [Fact]
    public void DetailedSpeechRecognitionResult_JsonSerialization_ShouldIncludeNBest()
    {
        // Arrange
        var result = new DetailedSpeechRecognitionResult
        {
            Text = "Hello world",

            NBest = new List<NBestResult>
            {
                new() { Display = "Hello world",  },
                new() { Display = "Hello word",  }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"text\":\"Hello world\"", json);
        Assert.Contains("\"nBest\":", json);
        Assert.Contains("\"Hello word\"", json);
    }

    [Fact]
    public void DetailedSpeechRecognitionResult_JsonDeserialization_ShouldDeserializeNBest()
    {
        // Arrange
        var json = """
        {
            "text": "Hello world",
            "nBest": [
                {
                    "display": "Hello world"
                },
                {
                    "display": "Hello word"
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<DetailedSpeechRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello world", result.Text);

        Assert.NotNull(result.NBest);
        Assert.Equal(2, result.NBest.Count);
        Assert.Equal("Hello world", result.NBest[0].Display);
        Assert.Equal("Hello word", result.NBest[1].Display);
    }
}
