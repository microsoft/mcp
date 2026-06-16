// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.HealthModels;

/// <summary>
/// Tests for <see cref="MonitorHealthModelService"/> that verify the control-plane and
/// data-plane requests are constructed correctly. These guard against regressions of the
/// HTTP 400 failure reported when querying entity health (see issue #2247).
/// </summary>
public class MonitorHealthModelServiceTests
{
    private const string SupportedApiVersion = "2025-05-01-preview";

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
