// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Search.Options.Index;
using Azure.Mcp.Tools.Search.Services;
using Azure.ResourceManager;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.KnowledgeBases.Models;
using Azure.Search.Documents.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Microsoft.Mcp.Core.Services.Caching;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Search.Tests.Service;

public class SearchServiceCacheTests
{
    private readonly ICacheService _cacheService;
    private readonly IAzureService _azureService;
    private readonly SearchService _service;

    public SearchServiceCacheTests()
    {
        _cacheService = Substitute.For<ICacheService>();
        _azureService = Substitute.For<IAzureService>();

        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        _azureService.CloudConfiguration.Returns(cloudConfig);

        _service = new SearchService(_cacheService, _azureService);
    }

    [Fact]
    public async Task ListServices_ReturnsCachedResult_WhenSubscriptionWideCacheHit()
    {
        // Arrange: cache already has data for this subscription
        var cached = new List<string> { "cached-svc" };
        _cacheService
            .GetAsync<List<string>>(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(cached);

        // Act
        var result = await _service.ListServices("sub123", cancellationToken: TestContext.Current.CancellationToken);

        // Assert: result comes from cache and no ARM call is made
        Assert.Equal(cached, result);
        await _azureService.DidNotReceive().GetSubscription(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListServices_RgCacheKey_IsDistinctFromSubscriptionWideKey()
    {
        // Arrange: both paths hit the cache so no ARM call is needed
        _cacheService
            .GetAsync<List<string>>("search", Arg.Is<string>(k => k.Contains("my-rg")), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(["rg-svc"]);
        _cacheService
            .GetAsync<List<string>>("search", Arg.Is<string>(k => !k.Contains("my-rg")), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
            .Returns(["sub-svc"]);

        // Act
        var subResult = await _service.ListServices("sub123", cancellationToken: TestContext.Current.CancellationToken);
        var rgResult = await _service.ListServices("sub123", resourceGroup: "my-rg", cancellationToken: TestContext.Current.CancellationToken);

        // Assert: results are distinct — the RG key and sub-wide key are different
        Assert.Equal(["sub-svc"], subResult);
        Assert.Equal(["rg-svc"], rgResult);

        // Verify the cache was queried with a key that includes "my-rg" for the RG call
        await _cacheService.Received(1).GetAsync<List<string>>(
            "search",
            Arg.Is<string>(k => k.Contains("my-rg")),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());

        // Verify the cache was queried with a key that excludes "my-rg" for the sub-wide call
        await _cacheService.Received(1).GetAsync<List<string>>(
            "search",
            Arg.Is<string>(k => !k.Contains("my-rg")),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>());
    }
}

public class SearchServiceTests
{
    [Fact]
    public async Task ProcessRetrieveResponse_IncludesResponseAndReferences_WhenAllPropertiesPresent()
    {
        var unfilteredJson = """
            {
              "response": [
                {
                  "content": []
                }
              ],
              "activity": [
                {
                  "type": "modelQueryPlanning",
                  "id": 0,
                  "inputTokens": 1968,
                  "outputTokens": 1822,
                  "elapsedMs": 9308
                }
              ],
              "references": [
                {
                  "type": "mcpTool",
                  "id": "0",
                  "activitySource": 2,
                  "sourceData": {
                    "title": "What is search?"
                  },
                  "rerankerScore": 3.5426497,
                  "toolName": "myMcpServerTool"
                }
              ],
              "other": "should be ignored"
            }
            """;

        var result = await InvokeProcessRetrieveResponse(unfilteredJson);

        Assert.Contains("\"response\"", result);
        Assert.Contains("\"references\"", result);
        Assert.DoesNotContain("\"activity\"", result);
        Assert.DoesNotContain("\"other\"", result);
    }

    [Fact]
    public async Task ProcessRetrieveResponse_IncludesOnlyResponse_WhenOnlyResponsePresent()
    {
        var unfilteredJson = """
            {
              "response": [
                {
                  "content": []
                }
              ],
              "activity": [
                {
                  "type": "modelQueryPlanning"
                }
              ],
              "other": "should be ignored"
            }
            """;

        var result = await InvokeProcessRetrieveResponse(unfilteredJson);

        Assert.Contains("\"response\"", result);
        Assert.DoesNotContain("\"references\"", result);
        Assert.DoesNotContain("\"activity\"", result);
        Assert.DoesNotContain("\"other\"", result);
    }

    [Fact]
    public async Task ProcessRetrieveResponse_ReturnsEmptyObject_WhenNoExpectedPropertiesPresent()
    {
        var unfilteredJson = """
            {
              "activity": [
                {
                  "type": "modelQueryPlanning"
                }
              ],
              "other": "should be ignored"
            }
            """;

        var result = await InvokeProcessRetrieveResponse(unfilteredJson);

        Assert.Equal("{}", result);
        Assert.DoesNotContain("\"activity\"", result);
        Assert.DoesNotContain("\"other\"", result);
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesIntentForMinimalReasoning_WhenMessagesProvided()
    {
        var messages = new List<(string role, string message)>
        {
            ("user", "Hello"),
            ("assistant", "How can I help?")
        };

        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(true, null, messages);

        var intent = Assert.IsType<KnowledgeRetrievalSemanticIntent>(request.Intents.Single());
        Assert.Equal("Hello\nHow can I help?", intent.Search);
        Assert.Empty(request.Messages);
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesIntentForMinimalReasoning_WhenOnlyQueryProvided()
    {
        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(true, "What is search?", null);

        var intent = Assert.IsType<KnowledgeRetrievalSemanticIntent>(request.Intents.Single());
        Assert.Equal("What is search?", intent.Search);
        Assert.Empty(request.Messages);
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesEmptyIntentForMinimalReasoning_WhenMessagesEmpty()
    {
        var messages = new List<(string role, string message)>();

        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(true, null, messages);

        var intent = Assert.IsType<KnowledgeRetrievalSemanticIntent>(request.Intents.Single());
        Assert.Equal(string.Empty, intent.Search);
        Assert.Empty(request.Messages);
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesQueryIntentForMinimalReasoning_WhenMessagesEmptyAndQueryProvided()
    {
        var messages = new List<(string role, string message)>();

        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(true, "Explain search", messages);

        var intent = Assert.IsType<KnowledgeRetrievalSemanticIntent>(request.Intents.Single());
        Assert.Equal("Explain search", intent.Search);
        Assert.Empty(request.Messages);
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesMessagesForStandardReasoning_WhenMessagesProvided()
    {
        var messages = new List<(string role, string message)>
        {
            ("user", "Show results"),
            ("assistant", "Sure")
        };

        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(false, null, messages);

        Assert.Empty(request.Intents);
        Assert.Collection(
            request.Messages,
            message =>
            {
                Assert.Equal("user", message.Role);
                var content = Assert.IsType<KnowledgeBaseMessageTextContent>(message.Content.Single());
                Assert.Equal("Show results", content.Text);
            },
            message =>
            {
                Assert.Equal("assistant", message.Role);
                var content = Assert.IsType<KnowledgeBaseMessageTextContent>(message.Content.Single());
                Assert.Equal("Sure", content.Text);
            });
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesQueryMessageForStandardReasoning_WhenNoMessagesProvided()
    {
        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(false, "Explain indexing", null);

        Assert.Empty(request.Intents);
        var message = Assert.Single(request.Messages);
        Assert.Equal("user", message.Role);
        var content = Assert.IsType<KnowledgeBaseMessageTextContent>(message.Content.Single());
        Assert.Equal("Explain indexing", content.Text);
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesEmptyQueryMessageForStandardReasoning_WhenMessagesEmpty()
    {
        var messages = new List<(string role, string message)>();

        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(false, null, messages);

        Assert.Empty(request.Intents);
        var message = Assert.Single(request.Messages);
        Assert.Equal("user", message.Role);
        var content = Assert.IsType<KnowledgeBaseMessageTextContent>(message.Content.Single());
        Assert.Equal(string.Empty, content.Text);
    }

    [Fact]
    public void BuildKnowledgeBaseRetrievalRequest_UsesQueryMessageForStandardReasoning_WhenMessagesEmptyAndQueryProvided()
    {
        var messages = new List<(string role, string message)>();

        var request = SearchService.BuildKnowledgeBaseRetrievalRequest(false, "Explain search", messages);

        Assert.Empty(request.Intents);
        var message = Assert.Single(request.Messages);
        Assert.Equal("user", message.Role);
        var content = Assert.IsType<KnowledgeBaseMessageTextContent>(message.Content.Single());
        Assert.Equal("Explain search", content.Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(IndexQueryType.Full)]
    public void ConfigureQueryType_UsesFullLuceneSyntax_ByDefault(IndexQueryType? queryType)
    {
        var options = new SearchOptions();

        SearchService.ConfigureQueryType(options, CreateIndex(), queryType);

        Assert.Equal(SearchQueryType.Full, options.QueryType);
        Assert.Null(options.SemanticSearch);
    }

    [Fact]
    public void ConfigureQueryType_UsesSimpleSyntax_WhenRequested()
    {
        var options = new SearchOptions();

        SearchService.ConfigureQueryType(options, CreateIndex(), IndexQueryType.Simple);

        Assert.Equal(SearchQueryType.Simple, options.QueryType);
        Assert.Null(options.SemanticSearch);
    }

    [Fact]
    public void ConfigureQueryType_UsesDefaultSemanticConfiguration_WhenSemanticRequested()
    {
        var index = CreateIndex();
        index.SemanticSearch = new SemanticSearch
        {
            DefaultConfigurationName = "default-config"
        };
        index.SemanticSearch.Configurations.Add(new SemanticConfiguration("default-config", new SemanticPrioritizedFields()));
        var options = new SearchOptions();

        SearchService.ConfigureQueryType(options, index, IndexQueryType.Semantic);

        Assert.Equal(SearchQueryType.Semantic, options.QueryType);
        Assert.Equal("default-config", options.SemanticSearch?.SemanticConfigurationName);
    }

    [Fact]
    public void ConfigureQueryType_UsesFirstSemanticConfiguration_WhenNoDefaultConfigured()
    {
        var index = CreateIndex();
        index.SemanticSearch = new SemanticSearch();
        index.SemanticSearch.Configurations.Add(new SemanticConfiguration("only-config", new SemanticPrioritizedFields()));
        var options = new SearchOptions();

        SearchService.ConfigureQueryType(options, index, IndexQueryType.Semantic);

        Assert.Equal(SearchQueryType.Semantic, options.QueryType);
        Assert.Equal("only-config", options.SemanticSearch?.SemanticConfigurationName);
    }

    [Fact]
    public void ConfigureQueryType_Throws_WhenSemanticRequestedWithoutConfiguration()
    {
        var options = new SearchOptions();

        var exception = Assert.Throws<InvalidOperationException>(
            () => SearchService.ConfigureQueryType(options, CreateIndex(), IndexQueryType.Semantic));

        Assert.Contains("semantic configuration", exception.Message);
    }

    private static SearchIndex CreateIndex() => new("test-index");

    private static async Task<string> InvokeProcessRetrieveResponse(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await SearchService.ProcessRetrieveResponse(stream);
    }
}
