// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Search.Services;
using Azure.Mcp.Tools.Search.Tests.TestSupport;
using Azure.ResourceManager;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Service;

// cspell:words knowledgebases knowledgesources
public sealed class KnowledgeBaseRetrievalTests
{
    [Theory]
    [InlineData("searchIndex")]
    [InlineData("azureBlob")]
    [InlineData("indexedSharePoint")]
    [InlineData("indexedOneLake")]
    [InlineData("indexedSql")]
    [InlineData("file")]
    [InlineData("web")]
    [InlineData("remoteSharePoint")]
    [InlineData("workIQ")]
    [InlineData("mcpServer")]
    [InlineData("fabricDataAgent")]
    [InlineData("fabricOntology")]
    public void CreateKnowledgeSourceParams_PreservesSourceKindAndName(string kind)
    {
        var source = ModelReaderWriter.Read<KnowledgeSource>(BinaryData.FromString(
            $$"""{"name":"test-source","kind":"{{kind}}"}"""))!;

        foreach (var includeSourceData in new[] { true, false })
        {
            var parameters = SearchService.CreateKnowledgeSourceParams(source.Name, source.GetType().Name, includeSourceData);
            using var json = JsonDocument.Parse(ModelReaderWriter.Write(parameters));
            var root = json.RootElement;

            Assert.Equal(kind, root.GetProperty("kind").GetString());
            Assert.Equal("test-source", root.GetProperty("knowledgeSourceName").GetString());
            Assert.True(root.GetProperty("includeReferences").GetBoolean());
            Assert.Equal(includeSourceData, root.GetProperty("includeReferenceSourceData").GetBoolean());
            Assert.False(root.TryGetProperty("alwaysQuerySource", out _));
            Assert.False(root.TryGetProperty("rerankerThreshold", out _));
        }
    }

    [Fact]
    public void CreateKnowledgeSourceParams_RejectsUnsupportedSourceKind()
    {
        var source = ModelReaderWriter.Read<KnowledgeSource>(
            BinaryData.FromString("""{"name":"test-source","kind":"unsupported"}"""))!;

        Assert.Throws<NotSupportedException>(() => SearchService.CreateKnowledgeSourceParams(source.Name, source.GetType().Name, true));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(null, true)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task RetrieveFromKnowledgeBase_SendsReferenceOptionsAndPreservesSourceData(
        bool? includeReferenceSourceData, bool useMinimalReasoning)
    {
        ConcurrentQueue<string> sourceRequests = [];
        string? retrievalRequest = null;
        const string retrievalResponse = """
            {
              "response": [{"content": [{"type": "text", "text": "An answer [ref_id:0]."}]}],
              "references": [{
                "type": "searchIndex",
                "id": "0",
                "activitySource": 0,
                "docKey": "doc1",
                "sourceData": {"title": "Original document", "url": "https://example.com/doc1"}
              }],
              "activity": [{"type": "searchIndex", "id": 0}]
            }
            """;
        using var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            string response;
            var path = request.RequestUri!.AbsolutePath;
            if (request.Method == HttpMethod.Post)
            {
                Assert.EndsWith("/retrieve", path);
                retrievalRequest = await request.Content!.ReadAsStringAsync(cancellationToken);
                response = retrievalResponse;
            }
            else if (path.Contains("knowledgebases", StringComparison.Ordinal))
            {
                var reasoning = useMinimalReasoning ? "minimal" : "low";
                response = $$"""
                    {
                      "name": "test-base",
                      "knowledgeSources": [{"name": "index-source"}, {"name": "web-source"}],
                      "retrievalReasoningEffort": {"kind": "{{reasoning}}"}
                    }
                    """;
            }
            else
            {
                Assert.Contains("knowledgesources", path);
                sourceRequests.Enqueue(path);
                response = path.Contains("index-source", StringComparison.Ordinal)
                    ? """{"name":"index-source","kind":"searchIndex","searchIndexParameters":{"searchIndexName":"products"}}"""
                    : """{"name":"web-source","kind":"web"}""";
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            };
        });
        using var httpClient = new HttpClient(handler);
        var cache = Substitute.For<ICacheService>();
        var service = CreateService(httpClient, cache);

        var result = await service.RetrieveFromKnowledgeBase(
            "test-search",
            "test-base",
            useMinimalReasoning ? "Find documents" : null,
            useMinimalReasoning ? null : [("user", "Find documents"), ("assistant", "With citations")],
            includeReferenceSourceData,
            TestContext.Current.CancellationToken);

        Assert.NotNull(retrievalRequest);
        using var requestJson = JsonDocument.Parse(retrievalRequest);
        var requestRoot = requestJson.RootElement;
        Assert.True(requestRoot.TryGetProperty(useMinimalReasoning ? "intents" : "messages", out _));
        if (includeReferenceSourceData.HasValue)
        {
            Assert.Equal(2, sourceRequests.Count);
            var parameters = requestRoot.GetProperty("knowledgeSourceParams").EnumerateArray().ToArray();
            Assert.Equal(2, parameters.Length);
            Assert.Equal("index-source", parameters[0].GetProperty("knowledgeSourceName").GetString());
            Assert.Equal("searchIndex", parameters[0].GetProperty("kind").GetString());
            Assert.Equal("web-source", parameters[1].GetProperty("knowledgeSourceName").GetString());
            Assert.Equal("web", parameters[1].GetProperty("kind").GetString());
            Assert.All(parameters, parameter =>
            {
                Assert.True(parameter.GetProperty("includeReferences").GetBoolean());
                Assert.Equal(includeReferenceSourceData.Value, parameter.GetProperty("includeReferenceSourceData").GetBoolean());
                Assert.False(parameter.TryGetProperty("alwaysQuerySource", out _));
            });
            await cache.Received(1).SetAsync(
                "search", SourceTypeCacheKey("https://test-search.search.windows.net/", "index-source"),
                nameof(SearchIndexKnowledgeSource), CacheDurations.ServiceData, Arg.Any<CancellationToken>());
            await cache.Received(1).SetAsync(
                "search", SourceTypeCacheKey("https://test-search.search.windows.net/", "web-source"),
                nameof(WebKnowledgeSource), CacheDurations.ServiceData, Arg.Any<CancellationToken>());
        }
        else
        {
            Assert.Empty(sourceRequests);
            Assert.False(requestRoot.TryGetProperty("knowledgeSourceParams", out _));
            await cache.DidNotReceive().GetAsync<string>(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
        }

        using var resultJson = JsonDocument.Parse(result);
        var reference = resultJson.RootElement.GetProperty("references")[0];
        Assert.Equal("0", reference.GetProperty("id").GetString());
        Assert.Equal("doc1", reference.GetProperty("docKey").GetString());
        Assert.Equal("Original document", reference.GetProperty("sourceData").GetProperty("title").GetString());
        Assert.Equal("https://example.com/doc1", reference.GetProperty("sourceData").GetProperty("url").GetString());
        Assert.False(resultJson.RootElement.TryGetProperty("activity", out _));
    }

    [Theory]
    [InlineData(403)]
    [InlineData(404)]
    public async Task RetrieveFromKnowledgeBase_PropagatesSourceMetadataErrors(int status)
    {
        using var handler = new StubHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            var isKnowledgeBase = request.RequestUri!.AbsolutePath.Contains("knowledgebases", StringComparison.Ordinal);
            var response = isKnowledgeBase
                ? """{"name":"test-base","knowledgeSources":[{"name":"index-source"}]}"""
                : """{"error":{"code":"SourceUnavailable","message":"Source metadata is unavailable."}}""";

            return Task.FromResult(new HttpResponseMessage(isKnowledgeBase ? HttpStatusCode.OK : (HttpStatusCode)status)
            {
                Content = new StringContent(response, System.Text.Encoding.UTF8, "application/json")
            });
        });
        using var httpClient = new HttpClient(handler);
        var service = CreateService(httpClient);

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            service.RetrieveFromKnowledgeBase(
                "test-search", "test-base", "Find documents", null, true, TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.Status);
    }

    [Fact]
    public async Task RetrieveFromKnowledgeBase_UsesCachedSourceTypesWithCurrentReferenceOptions()
    {
        List<bool[]> referenceOptions = [];
        using var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
                var parameters = json.RootElement.GetProperty("knowledgeSourceParams").EnumerateArray().ToArray();
                Assert.Equal(["searchIndex", "web"], parameters.Select(parameter => parameter.GetProperty("kind").GetString()));
                referenceOptions.Add([.. parameters.Select(parameter => parameter.GetProperty("includeReferenceSourceData").GetBoolean())]);
                return JsonResponse("""{"response":[],"references":[]}""");
            }

            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Contains("knowledgebases", request.RequestUri!.AbsolutePath);
            return JsonResponse(KnowledgeBaseResponse(["index-source", "web-source"]));
        });
        using var httpClient = new HttpClient(handler);
        var cache = Substitute.For<ICacheService>();
        cache.GetAsync<string>(
            "search", SourceTypeCacheKey("https://test-search.search.windows.net/", "index-source"),
            CacheDurations.ServiceData, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>(nameof(SearchIndexKnowledgeSource)));
        cache.GetAsync<string>(
            "search", SourceTypeCacheKey("https://test-search.search.windows.net/", "web-source"),
            CacheDurations.ServiceData, Arg.Any<CancellationToken>())
            .Returns(new ValueTask<string?>(nameof(WebKnowledgeSource)));
        var service = CreateService(httpClient, cache);

        foreach (var includeSourceData in new[] { true, false })
        {
            await service.RetrieveFromKnowledgeBase(
                "test-search", "test-base", "Find documents", null, includeSourceData, TestContext.Current.CancellationToken);
        }

        Assert.Equal([true, true], referenceOptions[0]);
        Assert.Equal([false, false], referenceOptions[1]);
    }

    private static string SourceTypeCacheKey(string endpoint, string sourceName) =>
        CacheKeyBuilder.Build("knowledge-source-types", endpoint, sourceName);

    private static string KnowledgeBaseResponse(IEnumerable<string> sourceNames) => $$"""
        {
          "name":"test-base",
          "knowledgeSources":[{{string.Join(",", sourceNames.Select(name => $$"""{"name":"{{name}}"}"""))}}],
          "retrievalReasoningEffort":{"kind":"minimal"}
        }
        """;

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
    };

    private static SearchService CreateService(
        HttpClient httpClient,
        ICacheService? cacheService = null)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-token", DateTimeOffset.MaxValue)));
        var cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        cloud.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);

        var azureService = Substitute.For<IAzureService>();
        azureService.CloudConfiguration.Returns(cloud);
        azureService.GetClient(Arg.Any<string?>()).Returns(httpClient);
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        return new SearchService(cacheService ?? Substitute.For<ICacheService>(), azureService);
    }
}
