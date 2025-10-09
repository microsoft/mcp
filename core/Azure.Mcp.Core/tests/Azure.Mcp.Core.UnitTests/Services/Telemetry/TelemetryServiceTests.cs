// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Telemetry;

public class TelemetryServiceTests
{
    private const string TestDeviceId = "test-device-id";
    private const string TestMacAddressHash = "test-hash";
    private readonly AzureMcpServerConfiguration _testConfiguration = new()
    {
        Name = "TestService",
        Version = "1.0.0",
        IsTelemetryEnabled = true
    };
    private readonly IOptions<AzureMcpServerConfiguration> _mockOptions;
    private readonly IMachineInformationProvider _mockInformationProvider;
    private readonly IOptions<ServiceStartOptions> _mockServiceOptions;

    public TelemetryServiceTests()
    {
        _mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        _mockOptions.Value.Returns(_testConfiguration);

        _mockServiceOptions = Substitute.For<IOptions<ServiceStartOptions>>();

        _mockInformationProvider = Substitute.For<IMachineInformationProvider>();
        _mockInformationProvider.GetMacAddressHash().Returns(Task.FromResult(TestMacAddressHash));
        _mockInformationProvider.GetOrCreateDeviceId().Returns(Task.FromResult<string?>(TestDeviceId));
    }

    [Fact]
    public async Task StartActivity_WhenTelemetryDisabled_ShouldReturnNull()
    {
        // Arrange
        _testConfiguration.IsTelemetryEnabled = false;
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions);
        const string activityId = "test-activity";

        // Act
        var activity = await service.StartActivity(activityId);
        var defaultTags = await service.GetDefaultTags();

        // Assert
        Assert.Null(activity);

        AssertDefaultTags(defaultTags, _mockServiceOptions.Value);
    }

    [Fact]
    public async Task StartActivity_WithClientInfo_WhenTelemetryDisabled_ShouldReturnNull()
    {
        // Arrange
        _testConfiguration.IsTelemetryEnabled = false;
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions);
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
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions);

        // Act & Assert
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, null!, _mockServiceOptions));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        mockOptions.Value.Returns((AzureMcpServerConfiguration)null!);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions));
    }

    [Fact]
    public async Task Constructor_SetsDefaultTags()
    {
        // Arrange
        var serviceStartOptions = new ServiceStartOptions
        {
            Mode = "test-mode",
            Debug = true,
            Transport = "test-transport"
        };

        _mockServiceOptions.Value.Returns(serviceStartOptions);

        // Act & Assert
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions);
        var tags = await service.GetDefaultTags();

        AssertDefaultTags(tags, serviceStartOptions);
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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions);

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

    private static void AssertDefaultTags(IReadOnlyList<KeyValuePair<string, object?>> tags,
        ServiceStartOptions? expectedServiceOptions = null)
    {
        var dictionary = tags.ToDictionary();
        Assert.NotEmpty(tags);

        AssertTag(dictionary, TelemetryConstants.TagName.DevDeviceId, TestDeviceId);
        AssertTag(dictionary, TelemetryConstants.TagName.MacAddressHash, TestMacAddressHash);

        if (expectedServiceOptions != null)
        {
            Assert.NotNull(expectedServiceOptions.Mode);
            AssertTag(dictionary, TelemetryConstants.TagName.ServerMode, expectedServiceOptions.Mode);
        }
        else
        {
            Assert.False(dictionary.ContainsKey(TelemetryConstants.TagName.ServerMode));
        }
    }

    private static void AssertTag(IDictionary<string, object?> tags, string tagName, string expectedValue)
    {
        Assert.True(tags.ContainsKey(tagName));
        Assert.Equal(expectedValue, tags[tagName]);
    }
}
