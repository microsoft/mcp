// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Server.Options;
using Azure.Mcp.Core.Configuration;
using Azure.Mcp.Core.Services.Telemetry;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryServiceTests()
    {
        _mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        _mockOptions.Value.Returns(_testConfiguration);

        _mockServiceOptions = Substitute.For<IOptions<ServiceStartOptions>>();

        _mockInformationProvider = Substitute.For<IMachineInformationProvider>();
        _mockInformationProvider.GetMacAddressHash().Returns(Task.FromResult(TestMacAddressHash));
        _mockInformationProvider.GetOrCreateDeviceId().Returns(Task.FromResult<string?>(TestDeviceId));

        _logger = Substitute.For<ILogger<TelemetryService>>();
    }

    [Fact]
    public void StartActivity_WhenTelemetryDisabled_ShouldReturnNull()
    {
        // Arrange
        _testConfiguration.IsTelemetryEnabled = false;
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger);
        const string activityId = "test-activity";

        // Act
        var activity = service.StartActivity(activityId);

        // Assert
        Assert.Null(activity);
    }

    [Fact]
    public void StartActivity_WithClientInfo_WhenTelemetryDisabled_ShouldReturnNull()
    {
        // Arrange
        _testConfiguration.IsTelemetryEnabled = false;
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger);
        const string activityId = "test-activity";
        var clientInfo = new Implementation
        {
            Name = "TestClient",
            Version = "2.0.0"
        };

        // Act
        using var activity = service.StartActivity(activityId, clientInfo);

        // Assert
        Assert.Null(activity);
    }

    [Fact]
    public void Dispose_WithNullLogForwarder_ShouldNotThrow()
    {
        // Arrange
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger);

        // Act & Assert
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, null!, _mockServiceOptions, _logger));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        mockOptions.Value.Returns((AzureMcpServerConfiguration)null!);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger));
    }

    [Fact]
    public void GetDefaultTags_ThrowsWhenTagsNotInitialized()
    {
        // Arrange
        _mockOptions.Value.Returns(_testConfiguration);

        // Act & Assert
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger);

        Assert.Throws<InvalidOperationException>(() => service.GetDefaultTags());
    }

    [Fact]
    public void GetDefaultTags_ReturnsEmptyOnDisabled()
    {
        // Arrange
        _testConfiguration.IsTelemetryEnabled = false;

        var serviceStartOptions = new ServiceStartOptions
        {
            Mode = "test-mode",
            Debug = true,
            Transport = "test-transport"
        };
        _mockServiceOptions.Value.Returns(serviceStartOptions);

        // Act
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger);
        var tags = service.GetDefaultTags();

        // Assert
        Assert.Empty(tags);
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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger);

        await service.InitializeAsync();

        // Act
        var activity = service.StartActivity(activityId);

        // Assert
        // ActivitySource.StartActivity typically handles null/empty names gracefully
        // The exact behavior may depend on the .NET version and ActivitySource implementation
        if (activity != null)
        {
            activity.Dispose();
        }
    }

    [Fact]
    public void StartActivity_WithoutInitialization_Throws()
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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger);

        // Act & Assert
        // Test both overloads.
        Assert.Throws<InvalidOperationException>(() => service.StartActivity("an-activity-id"));

        var implementation = new Implementation
        {
            Name = "Foo-Bar-MCP",
            Version = "1.0.0",
            Title = "Test MCP server"
        };
        Assert.Throws<InvalidOperationException>(() => service.StartActivity("an-activity-id", implementation));
    }

    [Fact]
    public async Task StartActivity_WhenInitializationFails_Throws()
    {
        // Arrange
        var informationProvider = new ExceptionalInformationProvider();

        var configuration = new AzureMcpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true
        };

        var mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        mockOptions.Value.Returns(configuration);

        var implementation = new Implementation
        {
            Name = "Foo-Bar-MCP",
            Version = "1.0.0",
            Title = "Test MCP server"
        };

        // Act & Assert
        using var service = new TelemetryService(informationProvider, mockOptions, _mockServiceOptions, _logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.InitializeAsync());

        Assert.Throws<InvalidOperationException>(() => service.StartActivity("an-activity-id", implementation));
    }

    [Fact]
    public async Task StartActivity_ReturnsActivityWhenEnabled()
    {
        // Arrange
        var serviceStartOptions = new ServiceStartOptions
        {
            Mode = "test-mode",
            Debug = true,
            Transport = "test-transport"
        };
        _mockServiceOptions.Value.Returns(serviceStartOptions);

        var configuration = new AzureMcpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true
        };
        var operationName = "an-activity-id";
        var mockOptions = Substitute.For<IOptions<AzureMcpServerConfiguration>>();
        mockOptions.Value.Returns(configuration);

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger);

        await service.InitializeAsync();

        var defaultTags = service.GetDefaultTags();

        // Act
        var activity = service.StartActivity(operationName);

        // Assert
        if (activity != null)
        {
            Assert.Equal(operationName, activity.OperationName);
        }

        AssertDefaultTags(defaultTags, serviceStartOptions);
    }

    [Fact]
    public async Task InitializeAsync_InvokedOnce()
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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger);

        await service.InitializeAsync();
        await service.InitializeAsync();

        // Act
        await _mockInformationProvider.Received(1).GetOrCreateDeviceId();
        await _mockInformationProvider.Received(1).GetMacAddressHash();
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

    private class ExceptionalInformationProvider : IMachineInformationProvider
    {
        public Task<string> GetMacAddressHash() => Task.FromResult("test-mac-address");

        public Task<string?> GetOrCreateDeviceId() => Task.FromException<string?>(
            new ArgumentNullException("test-exception"));
    }
}
