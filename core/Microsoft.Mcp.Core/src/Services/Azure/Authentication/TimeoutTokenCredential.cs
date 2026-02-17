// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;

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
        catch (AuthenticationFailedException ex) when (ex.Message.Contains("Interactive requests with mac broker enabled must be executed on the main thread on macOS", StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthenticationFailedException(
                "Authentication is not configured correctly." +
                "Please authenticate using Azure CLI ('az login'), Azure PowerShell ('Connect-AzAccount'), or Azure Developer CLI ('azd auth login') instead. " +
                "Alternatively, set AZURE_TOKEN_CREDENTIALS environment variable to use a specific credential provider.",
                ex);
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
        catch (AuthenticationFailedException ex) when (ex.Message.Contains("Interactive requests with mac broker enabled must be executed on the main thread on macOS", StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthenticationFailedException(
                "Authentication is not configured correctly." +
                "Please authenticate using Azure CLI ('az login'), Azure PowerShell ('Connect-AzAccount'), or Azure Developer CLI ('azd auth login') instead. " +
                "Alternatively, set AZURE_TOKEN_CREDENTIALS environment variable to use a specific credential provider.",
                ex);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Authentication timed out after {_timeout.TotalSeconds} seconds.");
        }
    }
}
