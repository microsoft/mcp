// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Commands.HealthModels.Entity;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels.Entity;

public class EntityGetHealthCommandTests : CommandUnitTestsBase<EntityGetHealthCommand, IMonitorHealthModelService>
{
    // Sample test data
    private const string TestEntity = "entity123";
    private const string TestHealthModel = "healthModel1";
    private const string TestResourceGroup = "resourceGroup1";
    private const string TestSubscription = "sub123";
    private const string TestTenant = "tenant123";
    private const string SupportedApiVersion = "2025-05-01-preview";

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsEntityHealth()
    {
        // Arrange
        JsonNode mockResponse = new JsonObject([new("entityId", "entity123"), new("health", "Healthy"), new("timestamp", "2023-05-01T12:00:00Z")]);

        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            TestTenant,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription,
            "--tenant", TestTenant);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            TestTenant,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredParameters_ReturnsBadRequest()
    {
        // Arrange & Act - missing entity parameter
        var result = await ExecuteCommandAsync(
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("required", result.Message, StringComparison.OrdinalIgnoreCase);

        // Verify service was not called
        await Service.DidNotReceive().GetEntityHealth(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EntityNotFound_ReturnsNotFound()
    {
        // Arrange
        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Entity not found"));

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidArgument_ReturnsBadRequest()
    {
        // Arrange
        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Invalid health model format"));

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Contains("Invalid argument", result.Message);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GeneralException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedError = "Unexpected error occurred";
        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesAuthMethodToService()
    {
        // Arrange
        var mockResponse = new JsonObject([new("entityId", "entity123"), new("health", "Healthy")]);
        var authMethod = AuthMethod.Credential.ToString().ToLowerInvariant();

        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Is(AuthMethod.Credential),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription,
            "--auth-method", authMethod);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Is(AuthMethod.Credential),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryPolicy_PassesRetryPolicyToService()
    {
        // Arrange
        var mockResponse = new JsonObject([new("entityId", "entity123"), new("health", "Healthy")]);
        const double RetryDelay = 3;
        const int MaxRetries = 5;

        Service.GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Is<RetryPolicyOptions>(r => r.DelaySeconds == RetryDelay && r.MaxRetries == MaxRetries),
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        var result = await ExecuteCommandAsync(
            "--entity", TestEntity,
            "--health-model", TestHealthModel,
            "--resource-group", TestResourceGroup,
            "--subscription", TestSubscription,
            "--retry-delay", RetryDelay.ToString(),
            "--retry-max-retries", MaxRetries.ToString());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);

        await Service.Received(1).GetEntityHealth(
            TestEntity,
            TestHealthModel,
            TestResourceGroup,
            TestSubscription,
            Arg.Any<AuthMethod?>(),
            Arg.Any<string>(),
            Arg.Is<RetryPolicyOptions>(r => r.DelaySeconds == RetryDelay && r.MaxRetries == MaxRetries),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("https://contoso.healthmodels.azure.com")]   // no trailing slash
    [InlineData("https://contoso.healthmodels.azure.com/")]  // trailing slash
    public async Task GetEntityHealth_BuildsWellFormedDataplaneUrl(string dataplaneEndpoint)
    {
        // Arrange
        const string entity = "2c139c0b-87d8-4935-ae18-4217431002d2";
        var handler = new CapturingHttpMessageHandler(request =>
        {
            var url = request.RequestUri!.ToString();
            if (url.Contains("management.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                var body = "{\"properties\":{\"dataplaneEndpoint\":\"" + dataplaneEndpoint + "\"}}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"history":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var service = CreateService(handler);

        // Act
        var result = await service.GetEntityHealth(
            entity,
            "contoso",
            "rg1",
            "12345678-1234-1234-1234-123456789012",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);

        var controlPlaneRequest = Assert.Single(handler.Requests, r => r.RequestUri!.Host == "management.azure.com");
        Assert.Contains($"api-version={SupportedApiVersion}", controlPlaneRequest.RequestUri!.Query, StringComparison.Ordinal);

        var dataplaneRequest = Assert.Single(handler.Requests, r => r.RequestUri!.Host == "contoso.healthmodels.azure.com");
        Assert.Equal($"https://contoso.healthmodels.azure.com/api/entities/{entity}/history", dataplaneRequest.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetEntityHealth_UrlEncodesEntityName()
    {
        // Arrange
        const string entity = "my entity/with special?chars";
        var handler = new CapturingHttpMessageHandler(request =>
        {
            var url = request.RequestUri!.ToString();
            if (url.Contains("management.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"properties":{"dataplaneEndpoint":"https://m.healthmodels.azure.com"}}""", Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"history":[]}""", Encoding.UTF8, "application/json")
            };
        });

        var service = CreateService(handler);

        // Act
        await service.GetEntityHealth(
            entity,
            "m",
            "rg1",
            "12345678-1234-1234-1234-123456789012",
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var dataplaneRequest = Assert.Single(handler.Requests, r => r.RequestUri!.Host == "m.healthmodels.azure.com");
        Assert.Equal(
            $"https://m.healthmodels.azure.com/api/entities/{Uri.EscapeDataString(entity)}/history",
            dataplaneRequest.RequestUri!.AbsoluteUri);
    }

    private static MonitorHealthModelService CreateService(CapturingHttpMessageHandler handler)
    {
        var tenantService = Substitute.For<ITenantService>();

        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        tenantService.CloudConfiguration.Returns(cloudConfig);

        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1))));
        tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler));

        return new MonitorHealthModelService(tenantService, httpClientFactory);
    }

    private sealed class CapturingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder = responder;

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_responder(request));
        }
    }
}
