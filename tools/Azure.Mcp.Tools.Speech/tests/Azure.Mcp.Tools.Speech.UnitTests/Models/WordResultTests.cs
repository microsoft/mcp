// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class WordResultTests
{
    [Fact]
    public void WordResult_DefaultValues_ShouldBeNull()
    {
        // Arrange & Act
        var result = new WordResult();

        // Assert
        Assert.Null(result.Word);
        Assert.Null(result.Offset);
        Assert.Null(result.Duration);

    }

    [Fact]
    public void WordResult_SetProperties_ShouldRetainValues()
    {
        // Arrange & Act
        var result = new WordResult
        {
            Word = "Hello",
            Offset = 1000,
            Duration = 500,

        };

        // Assert
        Assert.Equal("Hello", result.Word);
        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)500, result.Duration);

    }

    [Fact]
    public void WordResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = new WordResult
        {
            Word = "Hello",
            Offset = 1000,
            Duration = 500,

        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"word\":\"Hello\"", json);
        Assert.Contains("\"offset\":1000", json);
        Assert.Contains("\"duration\":500", json);
    }

    [Fact]
    public void WordResult_JsonDeserialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "word": "Hello",
            "offset": 1000,
            "duration": 500
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WordResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello", result.Word);
        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)500, result.Duration);

    }

    [Fact]
    public void WordResult_JsonDeserialization_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var json = """
        {
            "word": null,
            "offset": null,
            "duration": null
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WordResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Word);
        Assert.Null(result.Offset);
        Assert.Null(result.Duration);

    }
}
