// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Token credential used during test playback to avoid any interactive or real authentication.
/// Returns a deterministic sanitized token value accepted by the test proxy recordings.
/// </summary>
public sealed class PlaybackTokenCredential : TokenCredential
{
    private static readonly string s_tokenValue = "Sanitized"; // Matches proxy sanitizer expectations.
    private static readonly DateTimeOffset s_expiration = DateTimeOffset.UtcNow.AddHours(1);

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => new(s_tokenValue, s_expiration);

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        => ValueTask.FromResult(new AccessToken(s_tokenValue, s_expiration));
}
