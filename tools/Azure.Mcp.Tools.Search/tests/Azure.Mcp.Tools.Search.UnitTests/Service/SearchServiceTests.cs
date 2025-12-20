// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azure.Mcp.Tools.Search.Services;
using Azure.Search.Documents.KnowledgeBases.Models;
using Xunit;

namespace Azure.Mcp.Tools.Search.UnitTests.Service;

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

    private static async Task<string> InvokeProcessRetrieveResponse(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await SearchService.ProcessRetrieveResponse(stream);
    }
}
