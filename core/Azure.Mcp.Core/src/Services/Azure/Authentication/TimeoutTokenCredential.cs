// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

public class TimeoutTokenCredential(TokenCredential innerCredential, TimeSpan timeout) : TokenCredential
{
    private readonly TokenCredential _innerCredential = innerCredential;
    private readonly TimeSpan _timeout = timeout;

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        try
        {
            return _innerCredential.GetToken(requestContext, cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Authentication timed out after {_timeout.TotalSeconds} seconds.");
        }
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        try
        {
            return await _innerCredential.GetTokenAsync(requestContext, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Authentication timed out after {_timeout.TotalSeconds} seconds.");
        }
    }
}
