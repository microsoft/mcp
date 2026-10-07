// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
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
            var parameters = SearchService.CreateKnowledgeSourceParams(source, includeSourceData);
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

        Assert.Throws<NotSupportedException>(() => SearchService.CreateKnowledgeSourceParams(source, true));
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
        List<string> sourceRequests = [];
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
                sourceRequests.Add(path);
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
        var service = CreateService(httpClient);

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
        }
        else
        {
            Assert.Empty(sourceRequests);
            Assert.False(requestRoot.TryGetProperty("knowledgeSourceParams", out _));
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

    private static SearchService CreateService(HttpClient httpClient)
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
        return new SearchService(Substitute.For<ICacheService>(), azureService);
    }
}
