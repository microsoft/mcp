// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.Tests;

public sealed class KustoCustomCloudIntegrationTests
{
    [Fact]
    public async Task QueryItemsAsync_PublicEndpointsConfiguredAsCustom_UsesCustomConfiguration()
    {
        var path = WritePublicCloudMetadata();
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["cloud"] = "custom" })
                .Build();
            var runtimeConfiguration = Microsoft.Extensions.Options.Options.Create(
                new ServerRuntimeConfiguration { CustomCloudConfig = path });
            var cloudConfiguration = new AzureCloudConfiguration(configuration, runtimeConfiguration);
            Assert.Equal(AzureCloudConfiguration.AzureCloud.CustomCloud, cloudConfiguration.CloudType);
            var credential = Substitute.For<TokenCredential>();
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
            Uri? requestUri = null;
            var azureService = Substitute.For<IAzureService>();
            azureService.CloudConfiguration.Returns(cloudConfiguration);
            azureService.ResolveTenantIdAsync(null, Arg.Any<CancellationToken>()).Returns((string?)null);
            azureService.GetTokenCredentialAsync(null, Arg.Any<CancellationToken>()).Returns(credential);
            azureService.GetClient(KustoClient.HttpClientName).Returns(_ =>
                new HttpClient(new CallbackHttpMessageHandler(request =>
                {
                    requestUri = request.RequestUri;
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            """
                            {
                              "Tables": [
                                {
                                  "Columns": [
                                    { "ColumnName": "Message", "ColumnType": "string" }
                                  ],
                                  "Rows": [
                                    [ "custom cloud query succeeded" ]
                                  ]
                                }
                              ]
                            }
                            """)
                    };
                })));
            var service = new KustoService(azureService, Substitute.For<ICacheService>());

            var result = await service.QueryItemsAsync(
                "https://testcluster.kusto.windows.net",
                "database",
                "Table | take 1",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(
                new Uri("https://testcluster.kusto.windows.net/v1/rest/query"),
                requestUri);
            Assert.Equal("string", result[0].GetProperty("Message").GetString());
            Assert.Equal("custom cloud query succeeded", result[1][0].GetString());
            await credential.Received(1).GetTokenAsync(
                Arg.Is<TokenRequestContext>(context =>
                    Assert.Single(context.Scopes) == "https://kusto.kusto.windows.net/.default"),
                TestContext.Current.CancellationToken);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string WritePublicCloudMetadata()
    {
        var metadata = new Dictionary<string, object?>
        {
            ["authorityHost"] = "https://login.microsoftonline.com",
            ["arm"] = new Dictionary<string, object?>
            {
                ["endpoint"] = "https://management.azure.com",
                ["audience"] = "https://management.azure.com"
            },
            ["logAnalytics"] = new Dictionary<string, object?>
            {
                ["endpoint"] = "https://api.loganalytics.io",
                ["audience"] = "https://api.loganalytics.io"
            },
            ["kusto"] = new Dictionary<string, object?>
            {
                ["endpointSuffix"] = ".kusto.windows.net",
                ["audience"] = "https://kusto.kusto.windows.net"
            }
        };
        var path = Path.GetTempFileName();
        File.WriteAllText(path, JsonSerializer.Serialize(metadata));
        return path;
    }

    private sealed class CallbackHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> callback)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }
}
