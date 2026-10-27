// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

public interface IDataBoundaryResolver
{
    Task<string> ResolveAsync(CancellationToken cancellationToken);
}
