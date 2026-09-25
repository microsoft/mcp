// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Services.Azure;

public class BaseAzureResourceServiceTests
{
    [Fact]
    public void ResourceGraphQueryResult_ParsesPropertiesCorrectly()
    {
        var json = """
        {
            "totalRecords": 42,
            "count": 2,
            "data": [
                { "id": "/subscriptions/sub1/resourceGroups/rg1", "name": "res1" },
                { "id": "/subscriptions/sub1/resourceGroups/rg1", "name": "res2" }
            ],
            "resultTruncated": "true",
            "$skipToken": "skip-token-value"
        }
        """;

        var document = JsonDocument.Parse(json);
        using var result = new ResourceGraphQueryResult(document);

        Assert.Equal(42, result.TotalRecords);
        Assert.Equal(2, result.Count);
        Assert.True(result.IsTruncated);
        Assert.Equal("skip-token-value", result.SkipToken);
        Assert.Equal(JsonValueKind.Array, result.Data.ValueKind);
        Assert.Equal(2, result.Data.GetArrayLength());
        Assert.Same(document, result.Document);
    }

    [Fact]
    public void ResourceGraphQueryResult_HandlesBooleanTruncated()
    {
        var json = """
        {
            "totalRecords": 1,
            "count": 1,
            "data": [{ "id": "1" }],
            "resultTruncated": true
        }
        """;

        using var result = new ResourceGraphQueryResult(JsonDocument.Parse(json));
        Assert.True(result.IsTruncated);
        Assert.Null(result.SkipToken);
    }

    [Fact]
    public void ResourceGraphQueryResult_HandlesFalseTruncated()
    {
        var json = """
        {
            "count": 0,
            "data": [],
            "resultTruncated": "false"
        }
        """;

        using var result = new ResourceGraphQueryResult(JsonDocument.Parse(json));
        Assert.False(result.IsTruncated);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void ResourceGraphQueryResult_DisposesUnderlyingDocument()
    {
        var document = JsonDocument.Parse("{\"count\": 0, \"data\": []}");
        var result = new ResourceGraphQueryResult(document);
        result.Dispose();

        // Accessing RootElement on a disposed JsonDocument throws ObjectDisposedException
        Assert.Throws<ObjectDisposedException>(() => document.RootElement.GetProperty("count"));
    }

    [Fact]
    public void CreateResourceGraphRequestContent_SerializesOptionsCorrectly()
    {
        var queryContent = new ResourceQueryContent("resources | take 10")
        {
            Subscriptions = { "sub-1", "sub-2" },
            ManagementGroups = { "mg-1" },
            Options = new ResourceQueryRequestOptions
            {
                Top = 50,
                Skip = 10,
                SkipToken = "token-xyz",
                ResultFormat = ResultFormat.ObjectArray,
                AllowPartialScopes = true,
            },
        };

        var requestContent = TestAzureResourceService.CreateContent(queryContent);
        using var stream = new MemoryStream();
        requestContent.WriteTo(stream, TestContext.Current.CancellationToken);
        stream.Position = 0;

        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        Assert.Equal("resources | take 10", root.GetProperty("query").GetString());
        Assert.Equal(2, root.GetProperty("subscriptions").GetArrayLength());
        Assert.Equal("sub-1", root.GetProperty("subscriptions")[0].GetString());
        Assert.Equal("sub-2", root.GetProperty("subscriptions")[1].GetString());
        Assert.Equal(1, root.GetProperty("managementGroups").GetArrayLength());
        Assert.Equal("mg-1", root.GetProperty("managementGroups")[0].GetString());

        var options = root.GetProperty("options");
        Assert.Equal(50, options.GetProperty("$top").GetInt32());
        Assert.Equal(10, options.GetProperty("$skip").GetInt32());
        Assert.Equal("token-xyz", options.GetProperty("$skipToken").GetString());
        Assert.Equal("ObjectArray", options.GetProperty("resultFormat").GetString());
        Assert.True(options.GetProperty("allowPartialScopes").GetBoolean());
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WhenTenantResolves_ReturnsResolvedTenantId()
    {
        var azureService = Substitute.For<IAzureService>();
        azureService.ResolveTenantIdAsync("my-tenant", Arg.Any<CancellationToken>())
            .Returns("resolved-tenant-id");

        var service = new TestAzureResourceService(azureService);
        var result = await service.ResolveTenantIdPublicAsync("my-tenant", TestContext.Current.CancellationToken);

        Assert.Equal("resolved-tenant-id", result);
    }

    [Fact]
    public async Task ResolveTenantIdAsync_WhenTenantNull_FallsBackToDefaultTenant()
    {
        var expectedGuid = Guid.NewGuid();
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("fake-arm-token", DateTimeOffset.UtcNow.AddHours(1))));

        var payload = new
        {
            value = new[]
            {
                new
                {
                    id = $"/tenants/{expectedGuid}",
                    tenantId = expectedGuid.ToString()
                }
            }
        };

        var armHandler = new CapturingHttpMessageHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            }));

        var armOptions = new ArmClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(armHandler, disposeHandler: false))
        };
        armOptions.Retry.MaxRetries = 0;
        var armClient = new ArmClient(credential, defaultSubscriptionId: null, armOptions);

        var tenants = new List<TenantResource>();
        await foreach (var tenant in armClient.GetTenants().GetAllAsync(TestContext.Current.CancellationToken))
        {
            tenants.Add(tenant);
        }

        var azureService = Substitute.For<IAzureService>();
        azureService.ResolveTenantIdAsync(null, Arg.Any<CancellationToken>())
            .Returns((string?)null);
        azureService.GetTenants(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(tenants));

        var service = new TestAzureResourceService(azureService);
        var result = await service.ResolveTenantIdPublicAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(expectedGuid.ToString(), result);
    }

    private sealed class CapturingHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            responseFactory(request, cancellationToken);
    }

    private sealed class TestAzureResourceService(IAzureService azureService)
        : BaseAzureResourceService(azureService)
    {
        public static RequestContent CreateContent(ResourceQueryContent queryContent) =>
            CreateResourceGraphRequestContent(queryContent);

        public Task<string> ResolveTenantIdPublicAsync(string? tenant, CancellationToken cancellationToken) =>
            ResolveTenantIdAsync(tenant, cancellationToken);
    }
}
