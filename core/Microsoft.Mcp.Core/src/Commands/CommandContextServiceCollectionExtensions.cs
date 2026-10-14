// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Registers transport-independent command execution context services.
/// </summary>
public static class CommandContextServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="ICommandContextAccessor"/> for command loaders and services.
    /// </summary>
    /// <param name="services">The service collection for the host that executes the commands.</param>
    /// <returns>The same service collection, for chaining registrations.</returns>
    /// <remarks>
    /// Uses <see cref="CommandContextAccessor"/> unless an accessor registration already exists.
    /// Repeated calls preserve existing registrations. Register in each independent host's
    /// service collection; the default accessor does not share ambient state across providers.
    /// Registration alone does not establish execution scopes or enable endpoint validation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddCommandContextAccessor(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<ICommandContextAccessor, CommandContextAccessor>();
        return services;
    }
}
