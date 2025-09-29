// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Speech.Options;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Options;

public class SpeechOptionDefinitionsTests
{
    [Fact]
    public void EndpointName_ShouldHaveCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal("endpoint", SpeechOptionDefinitions.EndpointName);
    }

    [Fact]
    public void FileName_ShouldHaveCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal("file", SpeechOptionDefinitions.FileName);
    }

    [Fact]
    public void LanguageName_ShouldHaveCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal("language", SpeechOptionDefinitions.LanguageName);
    }

    [Fact]
    public void PhrasesName_ShouldHaveCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal("phrases", SpeechOptionDefinitions.PhrasesName);
    }

    [Fact]
    public void FormatName_ShouldHaveCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal("format", SpeechOptionDefinitions.FormatName);
    }

    [Fact]
    public void ProfanityName_ShouldHaveCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal("profanity", SpeechOptionDefinitions.ProfanityName);
    }

    [Fact]
    public void Endpoint_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var option = SpeechOptionDefinitions.Endpoint;

        // Assert
        Assert.NotNull(option);
        Assert.Equal("--endpoint", option.Name);
        Assert.True(option.Required);
        Assert.Contains("Azure AI Services endpoint URL", option.Description);
        Assert.Contains("cognitiveservices.azure.com", option.Description);
    }

    [Fact]
    public void File_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var option = SpeechOptionDefinitions.File;

        // Assert
        Assert.NotNull(option);
        Assert.Equal("--file", option.Name);
        Assert.True(option.Required);
        Assert.Contains("Path to the audio file", option.Description);
        Assert.Contains("recognize", option.Description);
    }

    [Fact]
    public void Language_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var option = SpeechOptionDefinitions.Language;

        // Assert
        Assert.NotNull(option);
        Assert.Equal("--language", option.Name);
        Assert.False(option.Required);
        Assert.Contains("language for speech recognition", option.Description);
    }

    [Fact]
    public void RequiredOptions_ShouldBeIdentified()
    {
        // Assert
        Assert.True(SpeechOptionDefinitions.Endpoint.Required);
        Assert.True(SpeechOptionDefinitions.File.Required);
    }

    [Fact]
    public void OptionalOptions_ShouldBeIdentified()
    {
        // Assert
        Assert.False(SpeechOptionDefinitions.Language.Required);
        Assert.False(SpeechOptionDefinitions.Format.Required);
        Assert.False(SpeechOptionDefinitions.Profanity.Required);
    }
}
