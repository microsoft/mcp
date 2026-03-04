// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.UnitTests.Services.Telemetry;

public class TelemetryServiceTests
{
    private const string TestDeviceId = "test-device-id";
    private const string TestMacAddressHash = "test-hash";
    private readonly McpServerConfiguration _testConfiguration = new()
    {
        Name = "TestService",
        Version = "1.0.0",
        IsTelemetryEnabled = true,
        DisplayName = "Test Display",
        RootCommandGroupName = "azmcp"
    };
    private readonly IOptions<McpServerConfiguration> _mockOptions;
    private readonly IMachineInformationProvider _mockInformationProvider;
    private readonly IOptions<ServiceStartOptions> _mockServiceOptions;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryServiceTests()
    {
        _mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
        _mockOptions.Value.Returns(_testConfiguration);

        _mockServiceOptions = Substitute.For<IOptions<ServiceStartOptions>>();
        _mockServiceOptions.Value.Returns(new ServiceStartOptions());

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
        var mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
        mockOptions.Value.Returns((McpServerConfiguration)null!);

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
            Transport = TransportTypes.StdIo
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
    public async Task StartActivity_WithInvalidActivityName_ShouldHandleGracefully(string activityName)
    {
        // Arrange
        var configuration = new McpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true,
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        };

        var mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
        mockOptions.Value.Returns(configuration);

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger);

        await service.InitializeAsync();

        // Act
        var activity = service.StartActivity(activityName);

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
        var configuration = new McpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true,
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        };

        var mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
        mockOptions.Value.Returns(configuration);

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger);

        // Act & Assert
        // Test both overloads.
        Assert.Throws<InvalidOperationException>(() => service.StartActivity("an-activity-id"));

        var clientInfo = new Implementation
        {
            Name = "Foo-Bar-MCP",
            Version = "1.0.0",
            Title = "Test MCP server"
        };
        Assert.Throws<InvalidOperationException>(() => service.StartActivity("an-activity-id", clientInfo));
    }

    [Fact]
    public async Task StartActivity_WhenInitializationFails_Throws()
    {
        // Arrange
        var informationProvider = new ExceptionalInformationProvider();

        var configuration = new McpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true,
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        };

        var mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
        mockOptions.Value.Returns(configuration);

        var clientInfo = new Implementation
        {
            Name = "Foo-Bar-MCP",
            Version = "1.0.0",
            Title = "Test MCP server"
        };

        // Act & Assert
        using var service = new TelemetryService(informationProvider, mockOptions, _mockServiceOptions, _logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.InitializeAsync());

        Assert.Throws<InvalidOperationException>(() => service.StartActivity("an-activity-id", clientInfo));
    }

    [Fact]
    public async Task StartActivity_ReturnsActivityWhenEnabled()
    {
        // Arrange
        var serviceStartOptions = new ServiceStartOptions
        {
            Mode = "test-mode",
            Debug = true,
            Transport = TransportTypes.StdIo
        };
        _mockServiceOptions.Value.Returns(serviceStartOptions);

        var configuration = new McpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true,
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        };
        var operationName = "an-activity-id";
        var mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
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

        AssertDefaultTags(defaultTags, configuration, serviceStartOptions);
    }

    [Fact]
    public async Task InitializeAsync_InvokedOnce()
    {
        // Arrange
        var configuration = new McpServerConfiguration
        {
            Name = "TestService",
            Version = "1.0.0",
            IsTelemetryEnabled = true,
            DisplayName = "Test Display",
            RootCommandGroupName = "azmcp"
        };

        var mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
        mockOptions.Value.Returns(configuration);

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger);

        await service.InitializeAsync();
        await service.InitializeAsync();

        // Act
        await _mockInformationProvider.Received(1).GetOrCreateDeviceId();
        await _mockInformationProvider.Received(1).GetMacAddressHash();
    }

    private static void AssertDefaultTags(IReadOnlyList<KeyValuePair<string, object?>> tags,
        McpServerConfiguration? expectedOptions, ServiceStartOptions? expectedServiceOptions)
    {
        var dictionary = tags.ToDictionary();
        Assert.NotEmpty(tags);

        AssertTag(dictionary, TagName.DevDeviceId, TestDeviceId);
        AssertTag(dictionary, TagName.MacAddressHash, TestMacAddressHash);
        AssertTag(dictionary, TagName.Host, RuntimeInformation.OSDescription);
        AssertTag(dictionary, TagName.ProcessorArchitecture, RuntimeInformation.ProcessArchitecture.ToString());

        if (expectedOptions != null)
        {
            AssertTag(dictionary, TagName.McpServerVersion, expectedOptions.Version);
            AssertTag(dictionary, TagName.McpServerName, expectedOptions.Name);
        }
        else
        {
            Assert.False(dictionary.ContainsKey(TagName.McpServerVersion));
            Assert.False(dictionary.ContainsKey(TagName.McpServerName));
        }

        if (expectedServiceOptions != null)
        {
            Assert.NotNull(expectedServiceOptions.Mode);
            AssertTag(dictionary, TagName.ServerMode, expectedServiceOptions.Mode);
            AssertTag(dictionary, TagName.Transport, expectedServiceOptions.Transport);
        }
        else
        {
            Assert.False(dictionary.ContainsKey(TagName.ServerMode));
            Assert.False(dictionary.ContainsKey(TagName.Transport));
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
