// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Tools.Postgres.Providers;

public interface IEntraTokenProvider
{
    Task<AccessToken> GetEntraToken(TokenCredential tokenCredential, CancellationToken cancellationToken);
}
