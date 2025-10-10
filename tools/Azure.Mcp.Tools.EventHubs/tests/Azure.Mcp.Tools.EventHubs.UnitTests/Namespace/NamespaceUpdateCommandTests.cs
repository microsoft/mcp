// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Azure.ResourceManager.EventHubs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.Namespace;

public class NamespaceUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<NamespaceUpdateCommand> _logger;
    private readonly NamespaceUpdateCommand _command;
    private readonly CommandContext _context;

    public NamespaceUpdateCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<NamespaceUpdateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventHubsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new NamespaceUpdateCommand(_logger);

        // Assert
        Assert.NotNull(command);
        Assert.Equal("update", command.Name);
        Assert.Equal("Create or Update Event Hubs Namespace", command.Title);
        Assert.Contains("Create or Update a Namespace", command.Description);
        Assert.True(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("", false, "Missing Required")]
    [InlineData("--subscription test-sub", false, "Missing Required")]
    [InlineData("--subscription test-sub --resource-group test-rg", false, "Missing Required")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns", false, "At least one update property must be provided")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns --sku-name Standard", true, "")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns --location eastus", true, "")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns --is-auto-inflate-enabled true", false, "When enabling auto-inflate, maximum-throughput-units must be specified")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns --is-auto-inflate-enabled true --maximum-throughput-units 20", true, "")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns --tags {\"env\":\"prod\"}", true, "")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed, string expectedErrorMessage)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (shouldSucceed)
        {
            var updatedNamespace = CreateSampleNamespace();
            _eventHubsService.CreateOrUpdateNamespaceAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<int?>(),
                Arg.Any<bool?>(),
                Arg.Any<int?>(),
                Arg.Any<bool?>(),
                Arg.Any<bool?>(),
                Arg.Any<Dictionary<string, string>?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(updatedNamespace);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Message);
            if (!string.IsNullOrEmpty(expectedErrorMessage))
            {
                Assert.Contains(expectedErrorMessage, response.Message);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesNamespaceWithSkuChanges()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--sku-name", "Premium",
            "--sku-tier", "Premium",
            "--sku-capacity", "4"
        ]);

        var updatedNamespace = CreateSampleNamespace();
        _eventHubsService.CreateOrUpdateNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null, // location
            "Premium",
            "Premium",
            4,
            null, // isAutoInflateEnabled
            null, // maximumThroughputUnits
            null, // kafkaEnabled
            null, // zoneRedundant
            null, // tags
            null, // tenant
            Arg.Any<RetryPolicyOptions?>())
            .Returns(updatedNamespace);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await _eventHubsService.Received(1).CreateOrUpdateNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null,
            "Premium",
            "Premium",
            4,
            null,
            null,
            null,
            null,
            null,
            null,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesNamespaceWithAutoInflateSettings()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--is-auto-inflate-enabled", "true",
            "--maximum-throughput-units", "20"
        ]);

        var updatedNamespace = CreateSampleNamespace();
        _eventHubsService.CreateOrUpdateNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            true,
            20,
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(updatedNamespace);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesNamespaceWithTags()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tags", "{\"environment\":\"production\",\"team\":\"platform\"}"
        ]);

        var expectedTags = new Dictionary<string, string>
        {
            { "environment", "production" },
            { "team", "platform" }
        };

        var updatedNamespace = CreateSampleNamespace();
        _eventHubsService.CreateOrUpdateNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Is<Dictionary<string, string>?>(tags =>
                tags != null &&
                tags.ContainsKey("environment") &&
                tags["environment"] == "production" &&
                tags.ContainsKey("team") &&
                tags["team"] == "platform"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(updatedNamespace);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidTagsJson()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tags", "invalid-json"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesNamespaceWithAllFeatures()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--location", "westus2",
            "--sku-name", "Premium",
            "--sku-tier", "Premium",
            "--sku-capacity", "2",
            "--kafka-enabled", "true",
            "--zone-redundant", "true",
            "--tags", "{\"env\":\"prod\"}"
        ]);

        var updatedNamespace = CreateSampleNamespace();
        _eventHubsService.CreateOrUpdateNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            "westus2",
            "Premium",
            "Premium",
            2,
            null,
            null,
            true,
            true,
            Arg.Any<Dictionary<string, string>?>(),
            null,
            Arg.Any<RetryPolicyOptions?>())
            .Returns(updatedNamespace);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await _eventHubsService.Received(1).CreateOrUpdateNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            "westus2",
            "Premium",
            "Premium",
            2,
            null,
            null,
            true,
            true,
            Arg.Any<Dictionary<string, string>?>(),
            null,
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--sku-name", "Premium"
        ]);

        _eventHubsService.CreateOrUpdateNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Update failed"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesKeyNotFoundException()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "nonexistent-namespace",
            "--sku-name", "Premium"
        ]);

        _eventHubsService.CreateOrUpdateNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new KeyNotFoundException("Namespace not found"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesCorrectParameters()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--sku-name", "Premium",
            "--tenant", "test-tenant-123"
        ]);

        var updatedNamespace = CreateSampleNamespace();
        _eventHubsService.CreateOrUpdateNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<Dictionary<string, string>?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(updatedNamespace);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _eventHubsService.Received(1).CreateOrUpdateNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null,
            "Premium",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "test-tenant-123",
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--location", "eastus",
            "--sku-name", "Standard",
            "--sku-tier", "Standard",
            "--sku-capacity", "1",
            "--is-auto-inflate-enabled", "true",
            "--maximum-throughput-units", "10",
            "--kafka-enabled", "false",
            "--zone-redundant", "true",
            "--tags", "{\"env\":\"test\"}",
            "--tenant", "test-tenant"
        ]);

        // Act
        var options = _command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, [parseResult]) as NamespaceUpdateOptions;

        // Assert
        Assert.NotNull(options);
        Assert.Equal("test-sub", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
        Assert.Equal("test-namespace", options.Namespace);
        Assert.Equal("eastus", options.Location);
        Assert.Equal("Standard", options.SkuName);
        Assert.Equal("Standard", options.SkuTier);
        Assert.Equal(1, options.SkuCapacity);
        Assert.True(options.IsAutoInflateEnabled);
        Assert.Equal(10, options.MaximumThroughputUnits);
        Assert.False(options.KafkaEnabled);
        Assert.True(options.ZoneRedundant);
        Assert.Equal("{\"env\":\"test\"}", options.Tags);
        Assert.Equal("test-tenant", options.Tenant);
    }

    private static Models.Namespace CreateSampleNamespace()
    {
        return new Models.Namespace(
            "test-namespace",
            "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace",
            "test-rg",
            "East US",
            new Models.EventHubsSku("Standard", "Standard", null),
            "Active",
            "Succeeded",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            "https://test-namespace.servicebus.windows.net:443/",
            "test-sub:test-namespace",
            false,
            null,
            true,
            false,
            new Dictionary<string, string> { { "env", "test" } });
    }
}
