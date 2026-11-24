// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server;

namespace Microsoft.Mcp.Core.Services.Host;

/// <inheritdoc />
public sealed class EnvironmentVariables(IOptions<ServerRuntimeConfiguration> configuration) : IEnvironmentVariables
{
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
        if (configuration.Value.IsHttpMode)
        {
            throw new UnauthorizedAccessException("Tool access to host environment variables is not allowed in HTTP mode.");
        }
    }
}
