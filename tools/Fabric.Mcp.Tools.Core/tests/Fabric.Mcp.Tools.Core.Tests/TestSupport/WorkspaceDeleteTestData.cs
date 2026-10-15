// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

public static class WorkspaceDeleteTestData
{
    public const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb27ff222";
    public const string Token = "test-token";

    public static TheoryData<string?> InvalidWorkspaceIds =>
    [
        (string?)null,
        "",
        " ",
        "not-a-uuid",
        "00000000-0000-0000-0000-000000000000",
        "../another-workspace",
        "https://untrusted.example/workspaces",
        $"{WorkspaceId}/items",
        $"{WorkspaceId}?unexpected=true",
        new string('x', 8192)
    ];

    public static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(Token, DateTimeOffset.MaxValue));
        return credential;
    }
}
