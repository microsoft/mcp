// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;

namespace Microsoft.Mcp.Core.Services.Host;

/// <inheritdoc />
public sealed class ProcessService(IOptions<ServerRuntimeConfiguration> configuration) : IProcessService
{
    public void EnsureAccess()
    {
        if (configuration.Value.IsHttpMode)
        {
            throw new UnauthorizedAccessException("Tool access to start host processes is not allowed in HTTP mode.");
        }
    }

    public bool Start(Process process)
    {
        EnsureAccess();
        return process.Start();
    }
}
