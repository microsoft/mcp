// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Fabric.Mcp.Tools.OneLake.Tests.TestSupport;

internal sealed class FakeTokenCredential : TokenCredential
{
    private readonly string _token;

    public FakeTokenCredential(string token = "fake-token")
    {
        _token = token;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken(_token, DateTimeOffset.UtcNow.AddMinutes(5));
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.UtcNow.AddMinutes(5)));
    }
}
