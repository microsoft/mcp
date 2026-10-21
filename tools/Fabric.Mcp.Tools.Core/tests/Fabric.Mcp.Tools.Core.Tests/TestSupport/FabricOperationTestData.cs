// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class FabricOperationTestData
{
    internal const string OperationId = "0acd697c-1550-43cd-b998-91bfbfbd47c6";
    internal const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff229";
    internal const string ItemId = "5b218778-e7a5-4d73-8187-f10824047715";
    internal static readonly DateTimeOffset Epoch = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

    internal static TokenCredential Credential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("operation-test-token", DateTimeOffset.MaxValue));
        return credential;
    }

    internal static HttpResponseMessage JsonResponse(string json, HttpStatusCode status = HttpStatusCode.OK, string? retryAfter = null)
    {
        var response = new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }
        return response;
    }

    internal static HttpResponseMessage ItemResponse(string workspaceId = WorkspaceId, string displayName = "Created", string type = "Lakehouse") =>
        JsonResponse(JsonSerializer.Serialize(new FabricItem
        {
            Id = ItemId,
            WorkspaceId = workspaceId,
            DisplayName = displayName,
            Type = type
        }, CoreJsonContext.Default.FabricItem), HttpStatusCode.Created);

    internal static HttpResponseMessage Accepted(string? operationId = OperationId, string? retryAfter = "20")
    {
        var response = new HttpResponseMessage(HttpStatusCode.Accepted);
        if (operationId is not null)
        {
            response.Headers.TryAddWithoutValidation("x-ms-operation-id", operationId);
        }
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }
        return response;
    }
}
