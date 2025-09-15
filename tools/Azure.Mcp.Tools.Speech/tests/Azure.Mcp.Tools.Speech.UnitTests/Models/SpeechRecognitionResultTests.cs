// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Models;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Models;

public class SpeechRecognitionResultTests
{
    [Fact]
    public void SpeechRecognitionResult_DefaultValues_ShouldBeNull()
    {
        // Arrange & Act
        var result = new SpeechRecognitionResult();

        // Assert
        Assert.Null(result.Text);
        Assert.Null(result.Confidence);
        Assert.Null(result.Offset);
        Assert.Null(result.Duration);
        Assert.Null(result.Language);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void SpeechRecognitionResult_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var result = new SpeechRecognitionResult();

        // Act
        result.Text = "Hello world";
        result.Confidence = 0.95;
        result.Offset = 1000;
        result.Duration = 2000;
        result.Language = "en-US";
        result.Reason = "RecognizedSpeech";

        // Assert
        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.95, result.Confidence);
        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)2000, result.Duration);
        Assert.Equal("en-US", result.Language);
        Assert.Equal("RecognizedSpeech", result.Reason);
    }

    [Fact]
    public void SpeechRecognitionResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = new SpeechRecognitionResult
        {
            Text = "Hello world",
            Confidence = 0.95,
            Offset = 1000,
            Duration = 2000,
            Language = "en-US",
            Reason = "RecognizedSpeech"
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"text\":\"Hello world\"", json);
        Assert.Contains("\"confidence\":0.95", json);
        Assert.Contains("\"offset\":1000", json);
        Assert.Contains("\"duration\":2000", json);
        Assert.Contains("\"language\":\"en-US\"", json);
        Assert.Contains("\"reason\":\"RecognizedSpeech\"", json);
    }

    [Fact]
    public void SpeechRecognitionResult_JsonDeserialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "text": "Hello world",
            "confidence": 0.95,
            "offset": 1000,
            "duration": 2000,
            "language": "en-US",
            "reason": "RecognizedSpeech"
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<SpeechRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.95, result.Confidence);
        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)2000, result.Duration);
        Assert.Equal("en-US", result.Language);
        Assert.Equal("RecognizedSpeech", result.Reason);
    }

    [Fact]
    public void SpeechRecognitionResult_JsonDeserialization_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var json = """
        {
            "text": null,
            "confidence": null,
            "offset": null,
            "duration": null,
            "language": null,
            "reason": null
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<SpeechRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Text);
        Assert.Null(result.Confidence);
        Assert.Null(result.Offset);
        Assert.Null(result.Duration);
        Assert.Null(result.Language);
        Assert.Null(result.Reason);
    }
}

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
            new() { Text = "Hello world", Confidence = 0.95 },
            new() { Text = "Hello word", Confidence = 0.85 }
        };

        // Act
        var result = new DetailedSpeechRecognitionResult
        {
            Text = "Hello world",
            Confidence = 0.95,
            NBest = nbestResults
        };

        // Assert
        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.95, result.Confidence);
        Assert.NotNull(result.NBest);
        Assert.Equal(2, result.NBest.Count);
        Assert.Equal("Hello world", result.NBest[0].Text);
        Assert.Equal(0.95, result.NBest[0].Confidence);
    }

    [Fact]
    public void DetailedSpeechRecognitionResult_JsonSerialization_ShouldIncludeNBest()
    {
        // Arrange
        var result = new DetailedSpeechRecognitionResult
        {
            Text = "Hello world",
            Confidence = 0.95,
            NBest = new List<NBestResult>
            {
                new() { Text = "Hello world", Confidence = 0.95 },
                new() { Text = "Hello word", Confidence = 0.85 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"text\":\"Hello world\"", json);
        Assert.Contains("\"confidence\":0.95", json);
        Assert.Contains("\"nBest\":", json);
        Assert.Contains("\"Hello word\"", json);
        Assert.Contains("0.85", json);
    }

    [Fact]
    public void DetailedSpeechRecognitionResult_JsonDeserialization_ShouldDeserializeNBest()
    {
        // Arrange
        var json = """
        {
            "text": "Hello world",
            "confidence": 0.95,
            "nBest": [
                {
                    "text": "Hello world",
                    "confidence": 0.95
                },
                {
                    "text": "Hello word",
                    "confidence": 0.85
                }
            ]
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<DetailedSpeechRecognitionResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.95, result.Confidence);
        Assert.NotNull(result.NBest);
        Assert.Equal(2, result.NBest.Count);
        Assert.Equal("Hello world", result.NBest[0].Text);
        Assert.Equal(0.95, result.NBest[0].Confidence);
        Assert.Equal("Hello word", result.NBest[1].Text);
        Assert.Equal(0.85, result.NBest[1].Confidence);
    }
}

public class NBestResultTests
{
    [Fact]
    public void NBestResult_DefaultValues_ShouldBeNull()
    {
        // Arrange & Act
        var result = new NBestResult();

        // Assert
        Assert.Null(result.Text);
        Assert.Null(result.Confidence);
        Assert.Null(result.Words);
    }

    [Fact]
    public void NBestResult_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var words = new List<WordResult>
        {
            new() { Word = "Hello", Confidence = 0.98 },
            new() { Word = "world", Confidence = 0.92 }
        };

        // Act
        var result = new NBestResult
        {
            Text = "Hello world",
            Confidence = 0.95,
            Words = words
        };

        // Assert
        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.95, result.Confidence);
        Assert.NotNull(result.Words);
        Assert.Equal(2, result.Words.Count);
        Assert.Equal("Hello", result.Words[0].Word);
        Assert.Equal(0.98, result.Words[0].Confidence);
    }

    [Fact]
    public void NBestResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = new NBestResult
        {
            Text = "Hello world",
            Confidence = 0.95,
            Words = new List<WordResult>
            {
                new() { Word = "Hello", Confidence = 0.98, Offset = 100, Duration = 500 },
                new() { Word = "world", Confidence = 0.92, Offset = 600, Duration = 400 }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"text\":\"Hello world\"", json);
        Assert.Contains("\"confidence\":0.95", json);
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
            "text": "Hello world",
            "confidence": 0.95,
            "words": [
                {
                    "word": "Hello",
                    "confidence": 0.98,
                    "offset": 100,
                    "duration": 500
                },
                {
                    "word": "world",
                    "confidence": 0.92,
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
        Assert.Equal("Hello world", result.Text);
        Assert.Equal(0.95, result.Confidence);
        Assert.NotNull(result.Words);
        Assert.Equal(2, result.Words.Count);
        Assert.Equal("Hello", result.Words[0].Word);
        Assert.Equal(0.98, result.Words[0].Confidence);
        Assert.Equal((ulong)100, result.Words[0].Offset);
        Assert.Equal((ulong)500, result.Words[0].Duration);
    }
}

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
        Assert.Null(result.Confidence);
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
            Confidence = 0.98
        };

        // Assert
        Assert.Equal("Hello", result.Word);
        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)500, result.Duration);
        Assert.Equal(0.98, result.Confidence);
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
            Confidence = 0.98
        };

        // Act
        var json = JsonSerializer.Serialize(result);

        // Assert
        Assert.Contains("\"word\":\"Hello\"", json);
        Assert.Contains("\"offset\":1000", json);
        Assert.Contains("\"duration\":500", json);
        Assert.Contains("\"confidence\":0.98", json);
    }

    [Fact]
    public void WordResult_JsonDeserialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
        {
            "word": "Hello",
            "offset": 1000,
            "duration": 500,
            "confidence": 0.98
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WordResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello", result.Word);
        Assert.Equal((ulong)1000, result.Offset);
        Assert.Equal((ulong)500, result.Duration);
        Assert.Equal(0.98, result.Confidence);
    }

    [Fact]
    public void WordResult_JsonDeserialization_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var json = """
        {
            "word": null,
            "offset": null,
            "duration": null,
            "confidence": null
        }
        """;

        // Act
        var result = JsonSerializer.Deserialize<WordResult>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Word);
        Assert.Null(result.Offset);
        Assert.Null(result.Duration);
        Assert.Null(result.Confidence);
    }
}
