// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Thrown when the server is configured to use the on-behalf-of (OBO) flow but the incoming
/// request was authenticated with an application-only token, which Microsoft Entra ID cannot
/// exchange for a downstream token.
/// </summary>
public sealed class OnBehalfOfUserTokenRequiredException()
    : InvalidOperationException(
        "This server uses the on-behalf-of (OBO) flow to call Azure, which requires the request to be authenticated with a delegated user access token. " +
        "The request was authenticated with an application-only token, which cannot be exchanged on behalf of a user. " +
        "Call the server with a user access token, or configure the server with '--outgoing-auth-strategy UseHostingEnvironmentIdentity' to call Azure using the hosting environment's identity.");
