// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;

namespace Microsoft.Mcp.Core.Services.Host;

/// <inheritdoc />
[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Guarded IProcessService implementation; process creation checks access before invoking platform APIs.")]
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
