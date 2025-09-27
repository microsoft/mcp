// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
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
        var services = CreateServiceProvider(setup);

        // Act & Assert (should not throw)
        var result = setup.RegisterCommands(services);
        Assert.NotNull(result);
    }

    [Fact]
    public void RegisterCommands_ShouldReturnSpeechGroup()
    {
        // Arrange
        var setup = new SpeechSetup();
        var services = CreateServiceProvider(setup);

        // Act
        var speechGroup = setup.RegisterCommands(services);

        // Assert
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
        var services = CreateServiceProvider(setup);

        // Act
        var speechGroup = setup.RegisterCommands(services);

        // Assert
        Assert.NotNull(speechGroup);
        Assert.Contains("Azure AI Services Speech", speechGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddSttSubGroup()
    {
        // Arrange
        var setup = new SpeechSetup();
        var services = CreateServiceProvider(setup);

        // Act
        var speechGroup = setup.RegisterCommands(services);

        // Assert
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
        var services = CreateServiceProvider(setup);

        // Act
        var speechGroup = setup.RegisterCommands(services);

        // Assert
        Assert.NotNull(speechGroup);

        var sttGroup = speechGroup.SubGroup.FirstOrDefault(g => g.Name == "stt");
        Assert.NotNull(sttGroup);

        Assert.True(sttGroup.Commands.ContainsKey("recognize"));
        var recognizeCommand = sttGroup.Commands["recognize"];
        Assert.NotNull(recognizeCommand);
    }

    [Fact]
    public void RegisterCommands_WithNullServiceProvider_ShouldThrow()
    {
        // Arrange
        var setup = new SpeechSetup();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => setup.RegisterCommands(null!));
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

        // Verify it has the RegisterCommands method with the correct signature
        var registerMethod = type.GetMethod("RegisterCommands", new[] { typeof(IServiceProvider) });
        Assert.NotNull(registerMethod);
        Assert.True(registerMethod.IsPublic);
        Assert.Equal(typeof(CommandGroup), registerMethod.ReturnType);
    }

    private static IServiceProvider CreateServiceProvider(SpeechSetup setup)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        // Add required dependencies
        services.AddSingleton(Substitute.For<ITenantService>());
        setup.ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
