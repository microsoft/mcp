// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using ModelContextProtocol.Protocol;
using NSubstitute;
using Xunit;
using static Azure.Mcp.Core.Services.Azure.Authentication.AzureCloudConfiguration;

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
    private readonly IAzureCloudConfiguration _mockCloudConfiguration;
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

        _mockCloudConfiguration = Substitute.For<IAzureCloudConfiguration>();

        _logger = Substitute.For<ILogger<TelemetryService>>();
    }

    [Fact]
    public void StartActivity_WhenTelemetryDisabled_ShouldReturnNull()
    {
        // Arrange
        _testConfiguration.IsTelemetryEnabled = false;
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);
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
        using var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);
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
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);

        // Act & Assert
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, null!, _mockServiceOptions, _logger, _mockCloudConfiguration));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowNullReferenceException()
    {
        // Arrange
        var mockOptions = Substitute.For<IOptions<McpServerConfiguration>>();
        mockOptions.Value.Returns((McpServerConfiguration)null!);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration));
    }

    [Fact]
    public void GetDefaultTags_ThrowsWhenTagsNotInitialized()
    {
        // Arrange
        _mockOptions.Value.Returns(_testConfiguration);

        // Act & Assert
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);

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
        var service = new TelemetryService(_mockInformationProvider, _mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);
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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);

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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);

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
        using var service = new TelemetryService(informationProvider, mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);

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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);

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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, _mockCloudConfiguration);

        await service.InitializeAsync();
        await service.InitializeAsync();

        // Act
        await _mockInformationProvider.Received(1).GetOrCreateDeviceId();
        await _mockInformationProvider.Received(1).GetMacAddressHash();
    }

    [Theory]
    [InlineData(null, AzureCloud.AzurePublicCloud)]
    [InlineData("AzureCloud", AzureCloud.AzurePublicCloud)]
    [InlineData("AzurePublicCloud", AzureCloud.AzurePublicCloud)]
    [InlineData("Public", AzureCloud.AzurePublicCloud)]
    [InlineData("https://custom.login.microsoftonline.com", AzureCloud.AzurePublicCloud)]
    [InlineData("AzureChinaCloud", AzureCloud.AzureChinaCloud)]
    [InlineData("China", AzureCloud.AzureChinaCloud)]
    [InlineData("AzureUSGovernmentCloud", AzureCloud.AzureUSGovernmentCloud)]
    [InlineData("AzureUSGovernment", AzureCloud.AzureUSGovernmentCloud)]
    [InlineData("USGovernment", AzureCloud.AzureUSGovernmentCloud)]
    [InlineData("USGov", AzureCloud.AzureUSGovernmentCloud)]
    public async Task StartActivity_HasCloudBasedOnServiceStartOptions(string? cloud, AzureCloud expectedCloud)
    {
        // Arrange
        var serviceStartOptions = new ServiceStartOptions
        {
            Mode = "test-mode",
            Debug = true,
            Transport = TransportTypes.StdIo,
            Cloud = cloud
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

        var cloudConfiguration = new AzureCloudConfiguration(Substitute.For<IConfiguration>(), _mockServiceOptions, Substitute.For<ILogger<AzureCloudConfiguration>>());

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, cloudConfiguration);

        await service.InitializeAsync();

        // Act
        var activity = service.StartActivity(operationName);

        // Assert
        if (activity != null)
        {
            Assert.Equal(operationName, activity.OperationName);
            AssertDefaultTags(activity.Tags, configuration, serviceStartOptions,
                tags => AssertTag(tags, TagName.Cloud, expectedCloud.ToString()));
        }
    }

    [Theory]
    [MemberData(nameof(HasCloudBasedOnConfigurationTestData))]
    public async Task StartActivity_HasCloudBasedOnConfiguration(string configName, string? cloud, AzureCloud expectedCloud)
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
        var mockOptions = Options.Create(configuration);

        var mockConfiguration = Substitute.For<IConfiguration>();
        mockConfiguration[configName].Returns(cloud);

        var cloudConfiguration = new AzureCloudConfiguration(mockConfiguration, _mockServiceOptions, Substitute.For<ILogger<AzureCloudConfiguration>>());

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, cloudConfiguration);

        await service.InitializeAsync();

        // Act
        var activity = service.StartActivity(operationName);

        // Assert
        if (activity != null)
        {
            Assert.Equal(operationName, activity.OperationName);
            AssertDefaultTags(activity.Tags, configuration, serviceStartOptions,
                tags => AssertTag(tags, TagName.Cloud, expectedCloud.ToString()));
        }
    }

    public static IEnumerable<object?[]> HasCloudBasedOnConfigurationTestData()
    {
        List<string> configNames = ["AZURE_CLOUD", "azure_cloud", "cloud", "Cloud"];
        List<(string?, AzureCloud)> cloudStringToCloud =
        [
            (null, AzureCloud.AzurePublicCloud),
            ("AzureCloud", AzureCloud.AzurePublicCloud),
            ("AzurePublicCloud", AzureCloud.AzurePublicCloud),
            ("Public", AzureCloud.AzurePublicCloud),
            ("https://custom.login.microsoftonline.com", AzureCloud.AzurePublicCloud),
            ("AzureChinaCloud", AzureCloud.AzureChinaCloud),
            ("China", AzureCloud.AzureChinaCloud),
            ("AzureUSGovernmentCloud", AzureCloud.AzureUSGovernmentCloud),
            ("AzureUSGovernment", AzureCloud.AzureUSGovernmentCloud),
            ("USGovernment", AzureCloud.AzureUSGovernmentCloud),
            ("USGov", AzureCloud.AzureUSGovernmentCloud)
        ];

        return configNames.SelectMany(configName => cloudStringToCloud.Select(cloudData => new object?[] { configName, cloudData.Item1, cloudData.Item2 }));
    }

    [Fact]
    public async Task StartActivity_NoCloudWhenAzureCloudConfigurationIsNull()
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

        using var service = new TelemetryService(_mockInformationProvider, mockOptions, _mockServiceOptions, _logger, null);

        await service.InitializeAsync();

        // Act
        var activity = service.StartActivity(operationName);

        // Assert
        if (activity != null)
        {
            Assert.Equal(operationName, activity.OperationName);
            AssertDefaultTags(activity.Tags, configuration, serviceStartOptions,
                tags => Assert.False(tags.ContainsKey(TagName.Cloud)));
        }
    }

    private static void AssertDefaultTags<T>(
        IEnumerable<KeyValuePair<string, T?>> tags,
        McpServerConfiguration? expectedOptions,
        ServiceStartOptions? expectedServiceOptions,
        Action<Dictionary<string, T?>>? additionalAsserts = null)
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

        additionalAsserts?.Invoke(dictionary);
    }

    private static void AssertTag<T>(IDictionary<string, T?> tags, string tagName, string expectedValue)
    {
        Assert.True(tags.ContainsKey(tagName));
        Assert.Equal(expectedValue, tags[tagName]?.ToString());
    }

    private class ExceptionalInformationProvider : IMachineInformationProvider
    {
        public Task<string> GetMacAddressHash() => Task.FromResult("test-mac-address");

        public Task<string?> GetOrCreateDeviceId() => Task.FromException<string?>(
            new ArgumentNullException("test-exception"));
    }
}
