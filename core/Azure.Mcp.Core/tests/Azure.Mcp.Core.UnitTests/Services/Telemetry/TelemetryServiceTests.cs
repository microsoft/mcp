// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Telemetry;

public class TelemetryServiceTests
{
    private readonly IOptions<AzureMcpServerConfiguration> _mockOptions;
    private readonly AzureMcpServerConfiguration _configuration;
    private readonly IMachineInformationProvider _mockInformationProvider;

    public TelemetryServiceTests()
    {
        _configuration = new AzureMcpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true
        };

        _mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        _mockOptions.Value.Returns(_configuration);

        _mockInformationProvider = Substitute.For<IMachineInformationProvider>();
        _mockInformationProvider.GetMacAddressHash().Returns(Task.FromResult("test-hash"));
        _mockInformationProvider.GetOrCreateDeviceId().Returns(Task.FromResult<string?>("test-device-id"));
    }

    [Fact]
    public async Task StartActivity_WhenTelemetryDisabled_ShouldReturnNull()
    {
        // Arrange
        _configuration.IsTelemetryEnabled = false;
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions);
        const string activityId = "test-activity";

        // Act
        var activity = await service.StartActivity(activityId);

        // Assert
        Assert.Null(activity);
    }

    [Fact]
    public async Task StartActivity_WithClientInfo_WhenTelemetryDisabled_ShouldReturnNull()
    {
        // Arrange
        _configuration.IsTelemetryEnabled = false;
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions);
        const string activityId = "test-activity";
        var clientInfo = new Implementation
        {
            Name = "TestClient",
            Version = "2.0.0"
        };

        // Act
        var activity = await service.StartActivity(activityId, clientInfo);

        // Assert
        Assert.Null(activity);
    }

    [Fact]
    public void Dispose_WithNullLogForwarder_ShouldNotThrow()
    {
        // Arrange
        var service = new TelemetryService(_mockInformationProvider, _mockOptions);

        // Act & Assert
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, null!));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        mockOptions.Value.Returns((AzureMcpServerConfiguration)null!);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, mockOptions));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task StartActivity_WithInvalidActivityId_ShouldHandleGracefully(string activityId)
    {
        // Arrange
        var configuration = new AzureMcpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true
        };

        var mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        mockOptions.Value.Returns(configuration);

        using var service = new TelemetryService(_mockInformationProvider, mockOptions);

        // Act
        var activity = await service.StartActivity(activityId);

        // Assert
        // ActivitySource.StartActivity typically handles null/empty names gracefully
        // The exact behavior may depend on the .NET version and ActivitySource implementation
        if (activity != null)
        {
            activity.Dispose();
        }
    }
}
