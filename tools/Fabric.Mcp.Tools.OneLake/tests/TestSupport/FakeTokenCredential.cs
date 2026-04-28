// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Fabric.Mcp.Tools.OneLake.Tests.TestSupport;

internal sealed class FakeTokenCredential(string token = "fake-token") : TokenCredential
{
    private readonly string _token = token;

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken(_token, DateTimeOffset.UtcNow.AddMinutes(5));
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.UtcNow.AddMinutes(5)));
    }
}
