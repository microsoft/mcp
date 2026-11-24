// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Mcp.Core.Areas.Server;

namespace Microsoft.Mcp.Core.Services.Host;

/// <summary>
/// Registers transport-aware services for tool access to the host.
/// </summary>
public static class HostServiceCollectionExtensions
{
    public static IServiceCollection AddHostServices(this IServiceCollection services)
    {
        services.AddOptions<ServerRuntimeConfiguration>();
        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.TryAddSingleton<IEnvironmentVariables, EnvironmentVariables>();
        services.TryAddSingleton<IProcessService, ProcessService>();
        return services;
    }
}
