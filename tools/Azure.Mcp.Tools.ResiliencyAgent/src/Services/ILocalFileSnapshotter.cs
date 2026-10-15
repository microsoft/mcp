// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

public interface ILocalFileSnapshotter
{
    ValidatedLocalFile Validate(string untrustedPath);

    Task<AgentAttachment> SnapshotAsync(
        ValidatedLocalFile file,
        CancellationToken cancellationToken);
}
