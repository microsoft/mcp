// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

public sealed class ResiliencyAgentServiceTests
{
    [Fact]
    public async Task SendAsync_AddsDataBoundaryAndInlineFileParts()
    {
        string? body = null;
        string? boundary = null;
        var handler = new RecordingHandler(async request =>
        {
            body = await request.Content!.ReadAsStringAsync();
            boundary = request.Headers.GetValues("DataBoundary").Single();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "jsonrpc": "2.0",
                      "id": "rpc-1",
                      "result": {
                        "id": "task-1",
                        "contextId": "conv-1",
                        "status": {
                          "state": "completed",
                          "message": {
                            "parts": [{ "kind": "text", "text": "Done." }]
                          }
                        },
                        "artifacts": []
                      }
                    }
                    """),
            };
        });
        using var client = new HttpClient(handler);
        IDataBoundaryResolver boundaryResolver = Substitute.For<IDataBoundaryResolver>();
        boundaryResolver.ResolveAsync(Arg.Any<CancellationToken>()).Returns("eu");
        using var service = new ResiliencyAgentService(
            client,
            boundaryResolver,
            NullLogger<ResiliencyAgentService>.Instance,
            "https://example.test/a2a",
            sendDataBoundary: true);
        byte[] content = [1, 2, 3];

        AgentTurn turn = await service.SendAsync(
            "conv-1",
            null,
            "Assess",
            [new AgentAttachment(
                "attachment-1",
                "architecture.png",
                "image/png",
                content,
                "hash",
                DateTimeOffset.UtcNow.AddHours(1))],
            CancellationToken.None);

        Assert.Equal("completed", turn.State);
        Assert.Equal("eu", boundary);
        using JsonDocument payload = JsonDocument.Parse(body!);
        JsonElement parts = payload.RootElement
            .GetProperty("params")
            .GetProperty("message")
            .GetProperty("parts");
        Assert.Equal(2, parts.GetArrayLength());
        Assert.Equal("text", parts[0].GetProperty("kind").GetString());
        Assert.Equal("file", parts[1].GetProperty("kind").GetString());
        JsonElement file = parts[1].GetProperty("file");
        Assert.Equal("bytes", file.GetProperty("kind").GetString());
        Assert.Equal("architecture.png", file.GetProperty("name").GetString());
        Assert.Equal("image/png", file.GetProperty("mimeType").GetString());
        Assert.Equal(Convert.ToBase64String(content), file.GetProperty("bytes").GetString());
    }

    [Fact]
    public void ReadArtifact_RendersGetZonalPostureEventHubReport()
    {
        const string resourceId =
            "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/demo-rg/" +
            "providers/microsoft.eventhub/namespaces/ehns-demo";
        using JsonDocument document = CreateDataArtifact(
            $$"""
            {
              "resiliencyAgentArtifactType": "GetZonalPosture",
              "summary": {
                "totalResources": 1,
                "resilientResources": 1,
                "nonResilientResources": 0
              },
              "resources": {
                "microsoft.eventhub/namespaces": [
                  {
                    "resourceId": "{{resourceId}}",
                    "status": "Enabled"
                  }
                ]
              }
            }
            """);

        var artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("text/markdown", artifact.MimeType);
        Assert.Equal(
            $$"""
            # Zonal Posture Report

            ## Summary

            | Total Resources | Resilient Resources | Non-Resilient Resources |
            |---|---|---|
            | 1 | 1 | 0 |

            ## Resources

            | Resource | Resource Type | Status |
            |---|---|---|
            | [ehns-demo](https://portal.azure.com/#@/resource{{resourceId}}) | microsoft.eventhub/namespaces | Enabled |
            """.ReplaceLineEndings("\n"),
            artifact.Content!.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void ReadArtifact_RendersMultipleResourceTypesAndResources()
    {
        using JsonDocument document = CreateDataArtifact(
            """
            {
              "resiliencyAgentArtifactType": "GetZonalPosture",
              "summary": {
                "totalResources": 3,
                "resilientResources": 2,
                "nonResilientResources": 1
              },
              "resources": {
                "microsoft.eventhub/namespaces": [
                  {
                    "resourceId": "\\subscriptions\\sub-1\\resourceGroups\\rg\\providers\\microsoft.eventhub\\namespaces\\events[primary]",
                    "status": "Enabled"
                  },
                  {
                    "resourceId": "/subscriptions/sub-1/resourceGroups/rg/providers/microsoft.eventhub/namespaces/events-secondary",
                    "status": "Needs | review"
                  }
                ],
                "Microsoft.Storage/storageAccounts": [
                  {
                    "resourceId": "/subscriptions/sub-1/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/storage (one)",
                    "status": "ZoneRedundant"
                  }
                ]
              }
            }
            """);

        var artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));
        string markdown = artifact.Content!;

        Assert.Contains(
            @"[events\[primary\]](https://portal.azure.com/#@/resource/subscriptions/sub-1/resourceGroups/rg/providers/microsoft.eventhub/namespaces/events%5Bprimary%5D)",
            markdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "[events-secondary](https://portal.azure.com/#@/resource/subscriptions/sub-1/resourceGroups/rg/providers/microsoft.eventhub/namespaces/events-secondary)",
            markdown,
            StringComparison.Ordinal);
        Assert.Contains("Needs \\| review", markdown, StringComparison.Ordinal);
        Assert.Contains(
            @"[storage \(one\)](https://portal.azure.com/#@/resource/subscriptions/sub-1/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/storage%20%28one%29)",
            markdown,
            StringComparison.Ordinal);
        Assert.Contains("| Microsoft.Storage/storageAccounts | ZoneRedundant |", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_UsesAbsoluteTenantNeutralPortalResourceLink()
    {
        using JsonDocument document = CreateDataArtifact(
            """
            {
              "resiliencyAgentArtifactType": "GetZonalPosture",
              "summary": {
                "totalResources": 1,
                "resilientResources": 1,
                "nonResilientResources": 0
              },
              "resources": {
                "type/name": [{
                  "resourceId": "/subscriptions/sub/resourceGroups/rg/providers/type/name/resource",
                  "status": "Enabled"
                }]
              }
            }
            """);

        var artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Contains(
            "https://portal.azure.com/#@/resource/subscriptions/sub/resourceGroups/rg/providers/type/name/resource",
            artifact.Content,
            StringComparison.Ordinal);
        Assert.DoesNotContain("#@", artifact.Content!.Replace("#@/resource", string.Empty), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(
        """
        {
          "resiliencyAgentArtifactType": "UnknownArtifact",
          "summary": { "totalResources": 9 },
          "unknownPayload": { "keepMe": "exact-value" }
        }
        """)]
    [InlineData(
        """
        {
          "resiliencyAgentArtifactType": "GetZonalPosture",
          "summary": {
            "totalResources": 1,
            "resilientResources": "malformed",
            "nonResilientResources": 0
          },
          "resources": {
            "microsoft.eventhub/namespaces": [{
              "resourceId": "/subscriptions/sub/providers/microsoft.eventhub/namespaces/ehns",
              "status": "Enabled",
              "extra": "retain-this"
            }]
          }
        }
        """)]
    [InlineData(
        """
        {
          "resiliencyAgentArtifactType": "GetZonalPosture",
          "summary": {
            "totalResources": 1,
            "resilientResources": 1
          },
          "resources": {
            "microsoft.eventhub/namespaces": [{
              "resourceId": "/subscriptions/sub/providers/microsoft.eventhub/namespaces/ehns",
              "status": "Enabled"
            }]
          },
          "diagnostic": "missing nonResilientResources"
        }
        """)]
    public void ReadArtifact_UnknownOrMalformedPostureUsesGenericNoLossFallback(string data)
    {
        using JsonDocument document = CreateDataArtifact(data);

        var artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));
        string markdown = artifact.Content!;

        Assert.Equal("text/markdown", artifact.MimeType);
        Assert.DoesNotContain("# Zonal Posture Report", markdown, StringComparison.Ordinal);
        Assert.Contains("resiliencyAgentArtifactType", markdown, StringComparison.Ordinal);

        using JsonDocument payload = JsonDocument.Parse(data);
        foreach (string scalar in EnumerateScalarText(payload.RootElement))
        {
            Assert.Contains(scalar, markdown, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ReadArtifact_TranscodesDataPartToReadableMarkdownAndPreservesUrls()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "zonal-posture-report",
              "description": "Assessment",
              "parts": [{
                "kind": "data",
                "data": {
                  "summary": {
                    "status": "Needs attention",
                    "portalUrl": "https://portal.azure.com/#resource/subscriptions/example"
                  },
                  "resources": [
                    { "name": "app-a", "zoneRedundant": false },
                    { "name": "app-b", "zoneRedundant": true }
                  ],
                  "unknownShape": { "nested": ["one", { "two": 2 }] }
                }
              }]
            }
            """);

        var artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("text/markdown", artifact.MimeType);
        Assert.Contains("## summary", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("- **status**: Needs attention", artifact.Content, StringComparison.Ordinal);
        Assert.Contains(
            "[https://portal.azure.com/#resource/subscriptions/example]" +
            "(https://portal.azure.com/#resource/subscriptions/example)",
            artifact.Content,
            StringComparison.Ordinal);
        Assert.Contains("| name | zoneRedundant |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("unknownShape", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("\"two\"", artifact.Content, StringComparison.Ordinal);
    }

    private static JsonDocument CreateDataArtifact(string data) =>
        JsonDocument.Parse(
            $$"""
            {
              "name": "zonal-posture-report",
              "description": "Assessment",
              "metadata": { "type": "resiliencyagent" },
              "parts": [{
                "kind": "data",
                "data": {{data}}
              }]
            }
            """);

    private static IEnumerable<string> EnumerateScalarText(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    yield return property.Name;
                    foreach (string value in EnumerateScalarText(property.Value))
                    {
                        yield return value;
                    }
                }

                break;

            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    foreach (string value in EnumerateScalarText(item))
                    {
                        yield return value;
                    }
                }

                break;

            case JsonValueKind.String:
                if (element.GetString() is string text && text.Length > 0)
                {
                    yield return text;
                }

                break;

            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                yield return element.GetRawText();
                break;
        }
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            handler(request);
    }
}
