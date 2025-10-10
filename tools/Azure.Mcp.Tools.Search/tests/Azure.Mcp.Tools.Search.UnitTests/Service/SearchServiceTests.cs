// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Mcp.Tools.Search.Services;
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

    private static async Task<string> InvokeProcessRetrieveResponse(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await SearchService.ProcessRetrieveResponse(stream);
    }
}
