// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models.Realtime;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class RealtimeRecognitionDetailedResultTests
{
    [Fact]
    public void RealtimeRecognitionDetailedResult_InheritsFromRealtimeRecognitionResult()
    {
        // Arrange & Act
        var result = new RealtimeRecognitionDetailedResult();

        // Assert
        Assert.IsAssignableFrom<RealtimeRecognitionResult>(result);
        Assert.Null(result.NBest);
    }

    [Fact]
    public void RealtimeRecognitionDetailedResult_WithRealtimeRecognitionNBestResults_ShouldRetainValues()
    {
        // Arrange
        var nBestResults = new List<RealtimeRecognitionNBestResult>
        {
            new() { Display = "Hello world",  },
            new() { Display = "Hello word",  }
        };

        // Act
        var result = new RealtimeRecognitionDetailedResult
        {
            Text = "Hello world",

            NBest = nBestResults
        };

        // Assert
        Assert.Equal("Hello world", result.Text);

        Assert.NotNull(result.NBest);
        Assert.Equal(2, result.NBest.Count);
        Assert.Equal("Hello world", result.NBest[0].Display);
    }

    [Fact]
    public void RealtimeRecognitionDetailedResult_JsonSerialization_ShouldIncludeNBest()
    {
        // Arrange
        var result = new RealtimeRecognitionDetailedResult
        {
            Text = "Hello world",

            NBest = new List<RealtimeRecognitionNBestResult>
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
    public void RealtimeRecognitionDetailedResult_JsonDeserialization_ShouldDeserializeNBest()
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
        var result = JsonSerializer.Deserialize<RealtimeRecognitionDetailedResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello world", result.Text);

        Assert.NotNull(result.NBest);
        Assert.Equal(2, result.NBest.Count);
        Assert.Equal("Hello world", result.NBest[0].Display);
        Assert.Equal("Hello word", result.NBest[1].Display);
    }
}
