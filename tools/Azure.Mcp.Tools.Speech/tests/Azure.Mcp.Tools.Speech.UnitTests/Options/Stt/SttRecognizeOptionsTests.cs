// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Speech.Options;
using Azure.Mcp.Tools.Speech.Options.Stt;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Options.Stt;

public class SttRecognizeOptionsTests
{
    [Fact]
    public void SttRecognizeOptions_ShouldInheritFromBaseSpeechOptions()
    {
        // Arrange & Act
        var options = new SttRecognizeOptions();

        // Assert
        Assert.IsAssignableFrom<BaseSpeechOptions>(options);
    }

    [Fact]
    public void AllProperties_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var options = new SttRecognizeOptions();

        // Assert
        Assert.Null(options.File);
        Assert.Null(options.Language);
        Assert.Null(options.Phrases);
        Assert.Null(options.Format);
        Assert.Null(options.Profanity);
        Assert.Null(options.Endpoint); // Inherited property
    }

    [Fact]
    public void File_ShouldBeSettable()
    {
        // Arrange
        var options = new SttRecognizeOptions();
        var expectedFile = "test-audio.wav";

        // Act
        options.File = expectedFile;

        // Assert
        Assert.Equal(expectedFile, options.File);
    }

    [Fact]
    public void Language_ShouldBeSettable()
    {
        // Arrange
        var options = new SttRecognizeOptions();
        var expectedLanguage = "en-US";

        // Act
        options.Language = expectedLanguage;

        // Assert
        Assert.Equal(expectedLanguage, options.Language);
    }

    [Fact]
    public void Phrases_ShouldBeSettable()
    {
        // Arrange
        var options = new SttRecognizeOptions();
        var expectedPhrases = new[] { "Azure", "cognitive services", "speech recognition" };

        // Act
        options.Phrases = expectedPhrases;

        // Assert
        Assert.Equal(expectedPhrases, options.Phrases);
    }

    [Fact]
    public void Format_ShouldBeSettable()
    {
        // Arrange
        var options = new SttRecognizeOptions();
        var expectedFormat = "detailed";

        // Act
        options.Format = expectedFormat;

        // Assert
        Assert.Equal(expectedFormat, options.Format);
    }

    [Fact]
    public void Profanity_ShouldBeSettable()
    {
        // Arrange
        var options = new SttRecognizeOptions();
        var expectedProfanity = "masked";

        // Act
        options.Profanity = expectedProfanity;

        // Assert
        Assert.Equal(expectedProfanity, options.Profanity);
    }

    [Fact]
    public void SttRecognizeOptions_ShouldSerializeToJson()
    {
        // Arrange
        var options = new SttRecognizeOptions
        {
            File = "test-audio.wav",
            Language = "en-US",
            Phrases = new[] { "Azure", "speech" },
            Format = "detailed",
            Profanity = "masked",
            Endpoint = "https://test.cognitiveservices.azure.com/",
            Subscription = "test-subscription-id"
        };

        // Act
        var json = JsonSerializer.Serialize(options);
        var deserializedOptions = JsonSerializer.Deserialize<SttRecognizeOptions>(json);

        // Assert
        Assert.NotNull(deserializedOptions);
        Assert.Equal(options.File, deserializedOptions.File);
        Assert.Equal(options.Language, deserializedOptions.Language);
        Assert.Equal(options.Phrases, deserializedOptions.Phrases);
        Assert.Equal(options.Format, deserializedOptions.Format);
        Assert.Equal(options.Profanity, deserializedOptions.Profanity);
        Assert.Equal(options.Endpoint, deserializedOptions.Endpoint);
        Assert.Equal(options.Subscription, deserializedOptions.Subscription);
    }

    [Fact]
    public void SttRecognizeOptions_ShouldDeserializeFromJson()
    {
        // Arrange
        var json = """
        {
            "file": "test-audio.wav",
            "language": "en-US",
            "phrases": ["Azure", "speech"],
            "format": "detailed",
            "profanity": "masked",
            "endpoint": "https://test.cognitiveservices.azure.com/",
            "subscription": "test-subscription-id"
        }
        """;

        // Act
        var options = JsonSerializer.Deserialize<SttRecognizeOptions>(json);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("test-audio.wav", options.File);
        Assert.Equal("en-US", options.Language);
        Assert.NotNull(options.Phrases);
        Assert.Equal(2, options.Phrases.Length);
        Assert.Contains("Azure", options.Phrases);
        Assert.Contains("speech", options.Phrases);
        Assert.Equal("detailed", options.Format);
        Assert.Equal("masked", options.Profanity);
        Assert.Equal("https://test.cognitiveservices.azure.com/", options.Endpoint);
        Assert.Equal("test-subscription-id", options.Subscription);
    }

    [Fact]
    public void SttRecognizeOptions_ShouldHandleNullValuesInJson()
    {
        // Arrange
        var json = """
        {
            "file": null,
            "language": null,
            "phrases": null,
            "format": null,
            "profanity": null,
            "endpoint": null,
            "subscription": "test-subscription-id"
        }
        """;

        // Act
        var options = JsonSerializer.Deserialize<SttRecognizeOptions>(json);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.File);
        Assert.Null(options.Language);
        Assert.Null(options.Phrases);
        Assert.Null(options.Format);
        Assert.Null(options.Profanity);
        Assert.Null(options.Endpoint);
        Assert.Equal("test-subscription-id", options.Subscription);
    }

    [Fact]
    public void SttRecognizeOptions_ShouldHandleMissingPropertiesInJson()
    {
        // Arrange
        var json = """
        {
            "subscription": "test-subscription-id"
        }
        """;

        // Act
        var options = JsonSerializer.Deserialize<SttRecognizeOptions>(json);

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.File);
        Assert.Null(options.Language);
        Assert.Null(options.Phrases);
        Assert.Null(options.Format);
        Assert.Null(options.Profanity);
        Assert.Null(options.Endpoint);
        Assert.Equal("test-subscription-id", options.Subscription);
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("detailed")]
    public void Format_ShouldAcceptValidValues(string format)
    {
        // Arrange & Act
        var options = new SttRecognizeOptions
        {
            Format = format
        };

        // Assert
        Assert.Equal(format, options.Format);
    }

    [Theory]
    [InlineData("masked")]
    [InlineData("removed")]
    [InlineData("raw")]
    public void Profanity_ShouldAcceptValidValues(string profanity)
    {
        // Arrange & Act
        var options = new SttRecognizeOptions
        {
            Profanity = profanity
        };

        // Assert
        Assert.Equal(profanity, options.Profanity);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("es-ES")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("zh-CN")]
    public void Language_ShouldAcceptValidLanguageCodes(string language)
    {
        // Arrange & Act
        var options = new SttRecognizeOptions
        {
            Language = language
        };

        // Assert
        Assert.Equal(language, options.Language);
    }

    [Fact]
    public void Phrases_ShouldAcceptEmptyArray()
    {
        // Arrange & Act
        var options = new SttRecognizeOptions
        {
            Phrases = Array.Empty<string>()
        };

        // Assert
        Assert.NotNull(options.Phrases);
        Assert.Empty(options.Phrases);
    }

    [Fact]
    public void Phrases_ShouldAcceptSinglePhrase()
    {
        // Arrange
        var singlePhrase = new[] { "Azure" };

        // Act
        var options = new SttRecognizeOptions
        {
            Phrases = singlePhrase
        };

        // Assert
        Assert.NotNull(options.Phrases);
        Assert.Single(options.Phrases);
        Assert.Equal("Azure", options.Phrases[0]);
    }

    [Fact]
    public void Phrases_ShouldAcceptMultiplePhrases()
    {
        // Arrange
        var multiplePhrases = new[] { "Azure", "cognitive services", "speech recognition", "artificial intelligence" };

        // Act
        var options = new SttRecognizeOptions
        {
            Phrases = multiplePhrases
        };

        // Assert
        Assert.NotNull(options.Phrases);
        Assert.Equal(4, options.Phrases.Length);
        Assert.Equal(multiplePhrases, options.Phrases);
    }

    [Fact]
    public void JsonPropertyNames_ShouldMatchConstants()
    {
        // Arrange
        var options = new SttRecognizeOptions
        {
            File = "test.wav",
            Language = "en-US",
            Phrases = new[] { "test" },
            Format = "simple",
            Profanity = "masked"
        };

        // Act
        var json = JsonSerializer.Serialize(options);

        // Assert
        Assert.Contains($"\"{SpeechOptionDefinitions.FileName}\"", json);
        Assert.Contains($"\"{SpeechOptionDefinitions.LanguageName}\"", json);
        Assert.Contains($"\"{SpeechOptionDefinitions.PhrasesName}\"", json);
        Assert.Contains($"\"{SpeechOptionDefinitions.FormatName}\"", json);
        Assert.Contains($"\"{SpeechOptionDefinitions.ProfanityName}\"", json);
    }

    [Fact]
    public void SttRecognizeOptions_ShouldSupportComplexScenario()
    {
        // Arrange
        var options = new SttRecognizeOptions
        {
            File = "/path/to/complex-audio-file.wav",
            Language = "en-US",
            Phrases = new[] { "Microsoft Azure", "artificial intelligence", "cognitive services", "speech-to-text" },
            Format = "detailed",
            Profanity = "removed",
            Endpoint = "https://eastus.api.cognitive.microsoft.com/",
            Subscription = "12345678-1234-5678-9012-123456789012"
        };

        // Act
        var json = JsonSerializer.Serialize(options);
        var roundTripOptions = JsonSerializer.Deserialize<SttRecognizeOptions>(json);

        // Assert
        Assert.NotNull(roundTripOptions);
        Assert.Equal(options.File, roundTripOptions.File);
        Assert.Equal(options.Language, roundTripOptions.Language);
        Assert.Equal(options.Phrases?.Length, roundTripOptions.Phrases?.Length);
        Assert.Equal(options.Format, roundTripOptions.Format);
        Assert.Equal(options.Profanity, roundTripOptions.Profanity);
        Assert.Equal(options.Endpoint, roundTripOptions.Endpoint);
        Assert.Equal(options.Subscription, roundTripOptions.Subscription);

        // Verify phrase content
        if (options.Phrases != null && roundTripOptions.Phrases != null)
        {
            for (int i = 0; i < options.Phrases.Length; i++)
            {
                Assert.Equal(options.Phrases[i], roundTripOptions.Phrases[i]);
            }
        }
    }
}
