// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Microsoft.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// SingleIdentityTokenCredentialProvider with an interactive browser prompt as a last resort, for
/// registry servers in the modes where the user's own identity drives auth.
/// </summary>
public sealed class RegistryBrowserFallbackTokenCredentialProvider(ILoggerFactory loggerFactory)
    : SingleIdentityTokenCredentialProvider(loggerFactory, forceBrowserFallback: true);
