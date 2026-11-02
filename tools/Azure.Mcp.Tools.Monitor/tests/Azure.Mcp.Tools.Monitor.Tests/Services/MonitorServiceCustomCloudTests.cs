// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Tests.TestSupport;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Services;

public sealed class MonitorServiceCustomCloudTests
{
    private const string Subscription = "00000000-0000-0000-0000-000000000001";
    private const string ResourceId = $"/subscriptions/{Subscription}/resourceGroups/test-rg/providers/Microsoft.Storage/storageAccounts/testaccount";
    private const string Tenant = "test-tenant";
    private const string ResolvedTenant = "00000000-0000-0000-0000-000000000002";
    private const string LogAnalyticsScope = "https://logs.contoso.example/.default";

    [Fact]
    public async Task QueryResourceLogs_CustomCloud_SendsExpectedRequestAndParsesResponse()
    {
        Uri? requestUri = null;
        string? requestBody = null;
        string? authorization = null;
        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            requestUri = request.RequestUri;
            requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            authorization = request.Headers.Authorization?.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "tables": [
                        {
                          "name": "PrimaryResult",
                          "columns": [
                            { "name": "TimeGenerated", "type": "datetime" },
                            { "name": "Count", "type": "long" },
                            { "name": "Active", "type": "bool" }
                          ],
                          "rows": [
                            [ "2026-09-08T12:00:00Z", 3, true ]
                          ]
                        }
                      ]
                    }
                    """)
            };
        });
        var (service, azureService, credential) = CreateService(handler);

        var results = await service.QueryResourceLogs(
            Subscription,
            ResourceId,
            "TestTable | project TimeGenerated, Count, Active",
            "TestTable",
            hours: 6,
            limit: 5,
            tenant: Tenant,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(
            $"https://logs.contoso.example/v1{ResourceId}/query?timespan=PT6H",
            requestUri!.AbsoluteUri);
        Assert.Equal("Bearer test-token", authorization);
        Assert.NotNull(requestBody);
        Assert.Contains("\"query\":\"TestTable | project TimeGenerated, Count, Active\\n| limit 5\"", requestBody);
        var result = Assert.Single(results);
        Assert.Equal("2026-09-08T12:00:00Z", result["TimeGenerated"]!.GetValue<string>());
        Assert.Equal(3, result["Count"]!.GetValue<int>());
        Assert.True(result["Active"]!.GetValue<bool>());
        await azureService.Received(1).ResolveTenantIdAsync(Tenant, Arg.Any<CancellationToken>());
        await azureService.Received(1).GetTokenCredentialAsync(ResolvedTenant, Arg.Any<CancellationToken>());
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context =>
                context.Scopes.Length == 1 && context.Scopes[0] == LogAnalyticsScope),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryResourceLogs_CustomCloudFailure_IncludesResponseBody()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("""{"error":"invalid audience"}""")
            }));
        var (service, _, _) = CreateService(handler);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.QueryResourceLogs(
                Subscription,
                ResourceId,
                "TestTable",
                "TestTable",
                hours: 1,
                limit: null,
                tenant: Tenant,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.Forbidden, exception.Status);
        Assert.Contains("""{"error":"invalid audience"}""", exception.Message);
    }

    private static (MonitorService Service, IAzureService AzureService, TokenCredential Credential) CreateService(
        HttpMessageHandler handler)
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.CustomCloud);
        cloudConfiguration.LogAnalyticsEndpoint.Returns(new Uri("https://logs.contoso.example"));
        cloudConfiguration.LogAnalyticsScope.Returns(LogAnalyticsScope);

        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));

        var azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloudConfiguration);
        azureService.ResolveTenantIdAsync(Tenant, Arg.Any<CancellationToken>()).Returns(ResolvedTenant);
        azureService.GetTokenCredentialAsync(ResolvedTenant, Arg.Any<CancellationToken>()).Returns(credential);
        azureService.GetClient().Returns(new HttpClient(handler));

        var service = new MonitorService(
            azureService,
            Substitute.For<IResourceResolverService>(),
            Substitute.For<ILogger<MonitorService>>());
        return (service, azureService, credential);
    }
}
