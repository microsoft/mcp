// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemJobInstanceTestData
{
    internal const string WorkspaceId = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
    internal const string ItemId = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";
    internal const string JobInstanceId = "cccccccc-cccc-4ccc-8ccc-cccccccccccc";
    internal const string ActivityId = "dddddddd-dddd-4ddd-8ddd-dddddddddddd";
    internal const string Json = """
        {
          "id": "cccccccc-cccc-4ccc-8ccc-cccccccccccc",
          "itemId": "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb",
          "jobType": "DefaultJob",
          "invokeType": "Manual",
          "status": "Completed",
          "rootActivityId": "dddddddd-dddd-4ddd-8ddd-dddddddddddd",
          "startTimeUtc": "2023-04-22T06:35:00.7812154",
          "endTimeUtc": "2023-04-22T06:35:00.8033333",
          "failureReason": null
        }
        """;

    internal static FabricItemJobInstance Instance() =>
        JsonSerializer.Deserialize(Json, CoreJsonContext.Default.FabricItemJobInstance)!;

    internal static TokenCredential Credential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("job-test-token", DateTimeOffset.MaxValue));
        return credential;
    }

    internal static HttpResponseMessage Response(string json = Json, HttpStatusCode status = HttpStatusCode.OK, string? retryAfter = null)
    {
        var response = new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }
        return response;
    }
}
