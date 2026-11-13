// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security.Claims;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Search.Services;
using Azure.ResourceManager;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Service;

public sealed class SearchServiceTenantTests
{
    private const string Tenant = "11111111-1111-1111-1111-111111111111";
    private const string OtherTenant = "22222222-2222-2222-2222-222222222222";
    private const string ServiceName = "test-search";

    [Theory]
    [InlineData("index")]
    [InlineData("indexes")]
    [InlineData("query")]
    [InlineData("base")]
    [InlineData("bases")]
    [InlineData("source")]
    [InlineData("sources")]
    [InlineData("retrieve")]
    public async Task DataPlaneOperations_SelectTenantWithoutArmDiscovery(string operation)
    {
        var provider = CreateCredentialProvider();
        var requests = new List<string>();
        using var handler = new SearchTestHttpMessageHandler(request =>
        {
            Assert.Equal("test-search.search.windows.net", request.RequestUri!.Host);
            requests.Add(request.RequestUri.AbsolutePath);
            return Respond(request);
        });
        using var client = new HttpClient(handler);
        var service = CreateService(new NoopCacheService(), provider, client);

        await ExecuteOperation(service, operation, Tenant);

        Assert.NotEmpty(requests);
        await provider.Received(operation == "retrieve" ? 2 : 1)
            .GetTokenCredentialAsync(Tenant, TestContext.Current.CancellationToken);
        await provider.DidNotReceive().GetTokenCredentialAsync(null, Arg.Any<CancellationToken>());
        if (operation == "retrieve")
        {
            Assert.Equal(2, requests.Count);
            Assert.Contains(requests, path => path.Contains("retrieve", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GetIndexDetails_OmittedTenant_UsesDefaultCredential(string? tenant)
    {
        var provider = CreateCredentialProvider();
        using var handler = new SearchTestHttpMessageHandler(Respond);
        using var client = new HttpClient(handler);
        var service = CreateService(new NoopCacheService(), provider, client);

        await service.GetIndexDetails(ServiceName, "products", tenant, TestContext.Current.CancellationToken);

        await provider.Received(1).GetTokenCredentialAsync(null, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetIndexDetails_CacheSeparatesTenantsAndDefaultCredential()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new SingleUserCliCacheService(memoryCache);
        var provider = CreateCredentialProvider();
        var tokens = new List<string?>();
        using var handler = new SearchTestHttpMessageHandler(request =>
        {
            tokens.Add(request.Headers.Authorization?.Parameter);
            return Respond(request);
        });
        using var client = new HttpClient(handler);
        var service = CreateService(cache, provider, client);

        foreach (var tenant in new string?[] { null, Tenant, OtherTenant, Tenant, null, OtherTenant })
        {
            await service.GetIndexDetails(ServiceName, "products", tenant, TestContext.Current.CancellationToken);
        }

        await provider.Received(1).GetTokenCredentialAsync(null, TestContext.Current.CancellationToken);
        await provider.Received(1).GetTokenCredentialAsync(Tenant, TestContext.Current.CancellationToken);
        await provider.Received(1).GetTokenCredentialAsync(OtherTenant, TestContext.Current.CancellationToken);
        Assert.Equal(["default", Tenant, OtherTenant, Tenant, "default", OtherTenant], tokens);
    }

    [Fact]
    public async Task GetIndexDetails_TenantNameAndId_UseSameResolvedTenantClient()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new SingleUserCliCacheService(memoryCache);
        var provider = CreateCredentialProvider();
        using var handler = new SearchTestHttpMessageHandler(request =>
        {
            if (request.RequestUri!.Host == "management.azure.com")
            {
                Assert.Equal("/tenants", request.RequestUri.AbsolutePath);
                return new(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""{"value":[{"id":"/tenants/{{Tenant}}","tenantId":"{{Tenant}}","displayName":"Target tenant"}]}""",
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            Assert.Equal(Tenant, request.Headers.Authorization?.Parameter);
            return Respond(request);
        });
        using var client = new HttpClient(handler);
        var service = CreateService(cache, provider, client);

        await service.GetIndexDetails(ServiceName, "products", "Target tenant", TestContext.Current.CancellationToken);
        await service.GetIndexDetails(ServiceName, "products", Tenant, TestContext.Current.CancellationToken);

        await provider.Received(1).GetTokenCredentialAsync(Tenant, TestContext.Current.CancellationToken);
        await provider.DidNotReceive().GetTokenCredentialAsync("Target tenant", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("query")]
    [InlineData("retrieve")]
    public async Task DataPlaneOperations_DeniedDefinitionRead_DoesNotFallBack(string operation)
    {
        var requests = 0;
        using var handler = new SearchTestHttpMessageHandler(request =>
        {
            requests++;
            Assert.Equal(HttpMethod.Get, request.Method);
            return new(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("""{"error":{"code":"Forbidden","message":"Definition access denied"}}""")
            };
        });
        using var client = new HttpClient(handler);
        var service = CreateService(new NoopCacheService(), CreateCredentialProvider(), client);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => ExecuteOperation(service, operation, Tenant));

        Assert.Equal(403, exception.Status);
        Assert.Equal("Forbidden", exception.ErrorCode);
        Assert.Equal(1, requests);
    }

    [Fact]
    public async Task GetIndexDetails_OboTenantMismatch_IsRejectedBeforeSearchRequest()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("tid", Tenant)], "test"))
        };
        var accessor = new HttpContextAccessor { HttpContext = context };
        var provider = new HttpOnBehalfOfTokenCredentialProvider(accessor, NullLogger<HttpOnBehalfOfTokenCredentialProvider>.Instance);
        using var handler = new SearchTestHttpMessageHandler(_ => throw new InvalidOperationException("No HTTP request should be sent."));
        using var client = new HttpClient(handler);
        var service = CreateService(new HttpServiceCacheService(), provider, client);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            service.GetIndexDetails(ServiceName, "products", OtherTenant, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("does not match the tenant of the authenticated user", exception.Message);
    }

    [Fact]
    public async Task GetIndexDetails_Cancellation_IsForwarded()
    {
        var provider = CreateCredentialProvider();
        using var handler = new SearchTestHttpMessageHandler(Respond);
        using var client = new HttpClient(handler);
        var service = CreateService(new NoopCacheService(), provider, client);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetIndexDetails(ServiceName, "products", Tenant, cancellation.Token));
    }

    private static IAzureTokenCredentialProvider CreateCredentialProvider()
    {
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var token = new AccessToken(call.ArgAt<string?>(0) ?? "default", DateTimeOffset.UtcNow.AddHours(1));
                var credential = Substitute.For<TokenCredential>();
                credential.GetToken(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>()).Returns(token);
                credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                    .Returns(ValueTask.FromResult(token));
                return credential;
            });
        return provider;
    }

    private static SearchService CreateService(ICacheService cache, IAzureTokenCredentialProvider provider, HttpClient client)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(client);
        var cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        cloud.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        var validator = new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor());
        // Use the real resolver path: any accidental tenant/subscription discovery reaches the
        // handler and fails its Search-only host assertion instead of being hidden by a mock.
        var azureService = new AzureService(cache, NullLogger<AzureService>.Instance,
            Substitute.For<ISubscriptionResolver>(), provider, factory, cloud, validator);
        return new SearchService(cache, azureService);
    }

    private static async Task ExecuteOperation(SearchService service, string operation, string? tenant)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        switch (operation)
        {
            case "index":
                Assert.Single(await service.GetIndexDetails(ServiceName, "products", tenant, cancellationToken));
                break;
            case "indexes":
                Assert.Empty(await service.GetIndexDetails(ServiceName, null, tenant, cancellationToken));
                break;
            case "query":
                Assert.Empty(await service.QueryIndex(ServiceName, "products", "*", tenant: tenant, cancellationToken: cancellationToken));
                break;
            case "bases":
                Assert.Empty(await service.ListKnowledgeBases(ServiceName, tenant: tenant, cancellationToken: cancellationToken));
                break;
            case "base":
                Assert.Single(await service.ListKnowledgeBases(ServiceName, "base", tenant, cancellationToken));
                break;
            case "sources":
                Assert.Empty(await service.ListKnowledgeSources(ServiceName, tenant: tenant, cancellationToken: cancellationToken));
                break;
            case "source":
                Assert.Single(await service.ListKnowledgeSources(ServiceName, "source", tenant, cancellationToken));
                break;
            case "retrieve":
                Assert.Contains("\"response\"", await service.RetrieveFromKnowledgeBase(
                    ServiceName, "base", "hi", null, null, tenant, cancellationToken));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    private static HttpResponseMessage Respond(HttpRequestMessage request)
    {
        Assert.Equal("test-search.search.windows.net", request.RequestUri!.Host);
        var path = request.RequestUri.AbsolutePath;
        var body = """{"value":[]}""";
        if (request.Method == HttpMethod.Post && path.Contains("retrieve", StringComparison.OrdinalIgnoreCase))
        {
            body = """{"response":[],"references":[]}""";
        }
        else if (request.Method == HttpMethod.Get && path.Contains("('products')", StringComparison.Ordinal))
        {
            body = """{"name":"products","fields":[]}""";
        }
        else if (request.Method == HttpMethod.Get && path.Contains("('base')", StringComparison.Ordinal))
        {
            body = """{"name":"base","knowledgeSources":[],"retrievalReasoningEffort":{"kind":"minimal"}}""";
        }
        else if (request.Method == HttpMethod.Get && path.Contains("('source')", StringComparison.Ordinal))
        {
            body = """{"name":"source","kind":"searchIndex","searchIndexParameters":{"searchIndexName":"products"}}""";
        }
        return new(HttpStatusCode.OK) { Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json") };
    }
}
