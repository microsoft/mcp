// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests;

public class SpeechSetupTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var setup = new SpeechSetup();

        // Assert
        Assert.NotNull(setup);
    }

    [Fact]
    public void RegisterCommands_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var setup = new SpeechSetup();
        var rootGroup = new CommandGroup("root", "Root command group");
        var loggerFactory = new NullLoggerFactory();

        // Act & Assert (should not throw)
        setup.RegisterCommands(rootGroup, loggerFactory);
    }

    [Fact]
    public void RegisterCommands_ShouldAddSpeechGroup()
    {
        // Arrange
        var setup = new SpeechSetup();
        var rootGroup = new CommandGroup("root", "Root command group");
        var loggerFactory = new NullLoggerFactory();

        // Act
        setup.RegisterCommands(rootGroup, loggerFactory);

        // Assert
        var speechGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "speech");
        Assert.NotNull(speechGroup);
        Assert.Equal("speech", speechGroup.Name);
        Assert.NotNull(speechGroup.Description);
        Assert.NotEmpty(speechGroup.Description);
    }

    [Fact]
    public void RegisterCommands_SpeechGroup_ShouldHaveCorrectDescription()
    {
        // Arrange
        var setup = new SpeechSetup();
        var rootGroup = new CommandGroup("root", "Root command group");
        var loggerFactory = new NullLoggerFactory();

        // Act
        setup.RegisterCommands(rootGroup, loggerFactory);

        // Assert
        var speechGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "speech");
        Assert.NotNull(speechGroup);
        Assert.Contains("Azure AI Services Speech", speechGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddSttSubGroup()
    {
        // Arrange
        var setup = new SpeechSetup();
        var rootGroup = new CommandGroup("root", "Root command group");
        var loggerFactory = new NullLoggerFactory();

        // Act
        setup.RegisterCommands(rootGroup, loggerFactory);

        // Assert
        var speechGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "speech");
        Assert.NotNull(speechGroup);

        var sttGroup = speechGroup.SubGroup.FirstOrDefault(g => g.Name == "stt");
        Assert.NotNull(sttGroup);
        Assert.Equal("stt", sttGroup.Name);
        Assert.Contains("Speech-to-text", sttGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddRecognizeCommand()
    {
        // Arrange
        var setup = new SpeechSetup();
        var rootGroup = new CommandGroup("root", "Root command group");
        var loggerFactory = new NullLoggerFactory();

        // Act
        setup.RegisterCommands(rootGroup, loggerFactory);

        // Assert
        var speechGroup = rootGroup.SubGroup.FirstOrDefault(g => g.Name == "speech");
        Assert.NotNull(speechGroup);

        var sttGroup = speechGroup.SubGroup.FirstOrDefault(g => g.Name == "stt");
        Assert.NotNull(sttGroup);

        Assert.True(sttGroup.Commands.ContainsKey("recognize"));
        var recognizeCommand = sttGroup.Commands["recognize"];
        Assert.NotNull(recognizeCommand);
    }

    [Fact]
    public void RegisterCommands_WithNullRootGroup_ShouldThrow()
    {
        // Arrange
        var setup = new SpeechSetup();
        var loggerFactory = new NullLoggerFactory();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => setup.RegisterCommands(null!, loggerFactory));
    }

    [Fact]
    public void SpeechSetup_TypeValidation_ShouldHaveCorrectProperties()
    {
        // Act
        var type = typeof(SpeechSetup);

        // Assert
        Assert.True(type.IsClass);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsInterface);

        // Verify it has a public constructor
        var constructors = type.GetConstructors();
        Assert.NotEmpty(constructors);

        // Verify it has the RegisterCommands method
        var registerMethod = type.GetMethod("RegisterCommands");
        Assert.NotNull(registerMethod);
        Assert.True(registerMethod.IsPublic);
    }
}
