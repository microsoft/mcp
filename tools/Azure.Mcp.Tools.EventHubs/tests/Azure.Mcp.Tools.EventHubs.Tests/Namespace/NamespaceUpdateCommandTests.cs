// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.EventHubs;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.Tests.Namespace;

public class NamespaceUpdateCommandTests : SubscriptionCommandUnitTestsBase<NamespaceUpdateCommand, IEventHubsService>
{
    [Theory]
    [InlineData(true, null, null, "Disabled", true)]
    [InlineData(true, true, true, "Enabled", false)]
    [InlineData(true, true, false, "Enabled", true)]
    [InlineData(false, null, null, "Enabled", false)]
    [InlineData(false, false, false, "Disabled", true)]
    public void NamespaceSecurity_DefaultsAndUpdates(bool isNew, bool? publicAccess, bool? sasAuth, string networkAccess, bool disableLocalAuth)
    {
        var data = new EventHubsNamespaceData("eastus") { PublicNetworkAccess = new("Enabled"), DisableLocalAuth = false };
        EventHubsService.ConfigureNamespaceSecurity(data, isNew, publicAccess, sasAuth);
        Assert.Equal(networkAccess, data.PublicNetworkAccess?.ToString());
        Assert.Equal(disableLocalAuth, data.DisableLocalAuth);
    }

    [Fact]
    public static async Task CreateOrUpdateNamespaceAsync_MergesExistingTags()
    {
        const string subscription = "00000000-0000-0000-0000-000000000000";
        const string resourceGroupId = $"/subscriptions/{subscription}/resourceGroups/test-rg";
        const string resourceGroupResponse = $$"""
            {"id":"{{resourceGroupId}}","name":"test-rg","location":"eastus"}
            """;
        const string namespaceResponse = $$"""
            {
              "id": "{{resourceGroupId}}/providers/Microsoft.EventHub/namespaces/test-namespace",
              "name": "test-namespace",
              "type": "Microsoft.EventHub/namespaces",
              "location": "eastus",
              "tags": {"environment":"test","owner":"existing-owner"},
              "sku": {"name":"Standard","tier":"Standard","capacity":1},
              "properties": {"provisioningState":"Succeeded","publicNetworkAccess":"Enabled","disableLocalAuth":false}
            }
            """;

        JsonElement? requestBody = null;
        using var handler = Substitute.For<HttpMessageHandler>();
        handler.ReturnsForAll(async callInfo =>
        {
            var request = callInfo.Arg<HttpRequestMessage>();
            if (request.Method == HttpMethod.Put)
            {
                using var document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken));
                requestBody = document.RootElement.Clone();
            }

            var responseBody = request.RequestUri!.AbsolutePath.EndsWith("/resourceGroups/test-rg", StringComparison.Ordinal)
                ? resourceGroupResponse
                : namespaceResponse;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, null, "application/json")
            };
        });
        using var httpClient = new HttpClient(handler);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var azureService = Substitute.For<IAzureService>();
        azureService.IsSubscriptionId(subscription).Returns(true);
        azureService.GetClient().Returns(httpClient);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        azureService.CloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        var service = new EventHubsService(azureService, NullLogger<EventHubsService>.Instance);

        await service.CreateOrUpdateNamespaceAsync(
            "test-namespace", "test-rg", subscription,
            tags: new() { ["environment"] = "production", ["project"] = "mcp" },
            cancellationToken: TestContext.Current.CancellationToken);

        var tags = Assert.IsType<JsonElement>(requestBody).GetProperty("tags");
        Assert.Equal("production", tags.GetProperty("environment").GetString());
        Assert.Equal("existing-owner", tags.GetProperty("owner").GetString());
        Assert.Equal("mcp", tags.GetProperty("project").GetString());
        Assert.Equal(3, tags.EnumerateObject().Count());
    }

    [Theory]
    [InlineData("--location eastus", null, null)]
    [InlineData("--enable-public-network-access true", true, null)]
    [InlineData("--enable-sas-authentication true", null, true)]
    [InlineData("--enable-public-network-access false --enable-sas-authentication false", false, false)]
    public async Task ExecuteAsync_ForwardsExplicitSecuritySettings(string options, bool? publicAccess, bool? sasAuth)
    {
        var response = await ExecuteCommandAsync($"--subscription test-sub --resource-group test-rg --namespace test-ns {options}");
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var call = Assert.Single(Service.ReceivedCalls());
        Assert.Equal(publicAccess, call.GetArguments()[13]);
        Assert.Equal(sasAuth, call.GetArguments()[14]);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("update", Command.Name);
        Assert.Equal("Create or Update Event Hubs Namespace", Command.Title);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.NotNull(Command.Description);
        Assert.NotEmpty(Command.Description);
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
        if (shouldSucceed)
        {
            var updatedNamespace = CreateSampleNamespace();
            Service.CreateOrUpdateNamespaceAsync(
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
                cancellationToken: Arg.Any<CancellationToken>())
                .Returns(updatedNamespace);
        }

        // Act
        var response = await ExecuteCommandAsync(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

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
        var updatedNamespace = CreateSampleNamespace();
        Service.CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(updatedNamespace);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--sku-name", "Premium",
            "--sku-tier", "Premium",
            "--sku-capacity", "4");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await Service.Received(1).CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesNamespaceWithAutoInflateSettings()
    {
        // Arrange
        var updatedNamespace = CreateSampleNamespace();
        Service.CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(updatedNamespace);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--is-auto-inflate-enabled", "true",
            "--maximum-throughput-units", "20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesNamespaceWithTags()
    {
        // Arrange
        var expectedTags = new Dictionary<string, string>
        {
            { "environment", "production" },
            { "team", "platform" }
        };

        var updatedNamespace = CreateSampleNamespace();
        Service.CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(updatedNamespace);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tags", "{\"environment\":\"production\",\"team\":\"platform\"}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesInvalidTagsJson()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tags", "invalid-json");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("Invalid tags JSON format", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesNamespaceWithAllFeatures()
    {
        // Arrange
        var updatedNamespace = CreateSampleNamespace();
        Service.CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(updatedNamespace);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--location", "westus2",
            "--sku-name", "Premium",
            "--sku-tier", "Premium",
            "--sku-capacity", "2",
            "--kafka-enabled", "true",
            "--zone-redundant", "true",
            "--tags", "{\"env\":\"prod\"}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await Service.Received(1).CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        Service.CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Update failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--sku-name", "Premium");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesKeyNotFoundException()
    {
        // Arrange
        Service.CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Namespace not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "nonexistent-namespace",
            "--sku-name", "Premium");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithTenant_PassesCorrectParameters()
    {
        // Arrange
        var updatedNamespace = CreateSampleNamespace();
        Service.CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(updatedNamespace);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--sku-name", "Premium",
            "--tenant", "test-tenant-123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateOrUpdateNamespaceAsync(
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
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = CommandDefinition.Parse([
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
        var options = Command.BindOptions(parseResult);

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
        => new(
            "test-namespace",
            "/subscriptions/test-sub/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace",
            "test-rg",
            "East US",
            new("Standard", "Standard", null),
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
