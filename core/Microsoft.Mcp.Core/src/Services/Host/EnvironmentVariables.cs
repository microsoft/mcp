// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;

namespace Microsoft.Mcp.Core.Services.Host;

/// <inheritdoc />
[SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Guarded IEnvironmentVariables implementation; each host operation checks access before invoking platform APIs.")]
public sealed class EnvironmentVariables(IOptions<ServerRuntimeConfiguration> configuration) : IEnvironmentVariables
{
    public bool IsAccessAllowed => !configuration.Value.IsHttpMode;

    public string? GetEnvironmentVariable(string name)
    {
        EnsureAccess();
        return Environment.GetEnvironmentVariable(name);
    }

    public string GetFolderPath(Environment.SpecialFolder folder)
    {
        EnsureAccess();
        return Environment.GetFolderPath(folder);
    }

    private void EnsureAccess()
    {
        if (!IsAccessAllowed)
        {
            throw new UnauthorizedAccessException("Tool access to host environment variables is not allowed in HTTP mode.");
        }
    }
}
