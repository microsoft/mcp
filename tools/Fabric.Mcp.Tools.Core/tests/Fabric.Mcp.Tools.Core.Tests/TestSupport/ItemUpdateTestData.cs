// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class ItemUpdateTestData
{
    internal const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff229";
    internal const string ItemId = "5b218778-e7a5-4d73-8187-f10824047715";
    internal const string PrivateDetails = "private-backend-details";
    internal const string Token = "test-token";

    internal static ItemUpdateMetadata Metadata(string? description = null) =>
        new(ItemId, "Updated item", "Lakehouse", WorkspaceId, description);

    internal static HttpResponseMessage Response(HttpStatusCode status = HttpStatusCode.OK, string? json = null) =>
        new(status)
        {
            Content = new StringContent(
                json ?? JsonSerializer.Serialize(Metadata(), CoreJsonContext.Default.ItemUpdateMetadata),
                Encoding.UTF8,
                "application/json")
        };

    internal static TokenCredential Credential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(Token, DateTimeOffset.MaxValue));
        return credential;
    }
}
