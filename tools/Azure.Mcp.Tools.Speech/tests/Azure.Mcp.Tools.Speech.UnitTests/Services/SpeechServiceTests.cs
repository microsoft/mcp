// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Speech.UnitTests.Services;

public class SpeechServiceTests
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<SpeechService> _logger;
    private readonly SpeechService _speechService;

    public SpeechServiceTests()
    {
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<SpeechService>>();

        _speechService = new SpeechService(_tenantService, _logger);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new SpeechService(_tenantService, _logger);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void SpeechService_TypeValidation_ShouldHaveCorrectInterfaces()
    {
        // Act
        var type = typeof(SpeechService);

        // Assert
        Assert.True(type.IsClass);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsInterface);

        // Verify it has a public constructor
        var constructors = type.GetConstructors();
        Assert.NotEmpty(constructors);

        // Verify it has the RecognizeSpeechFromFile method
        var recognizeMethod = type.GetMethod("RecognizeSpeechFromFile");
        Assert.NotNull(recognizeMethod);
        Assert.True(recognizeMethod.IsPublic);
    }
}
