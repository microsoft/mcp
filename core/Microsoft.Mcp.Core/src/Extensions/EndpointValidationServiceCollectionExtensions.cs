// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;

namespace Microsoft.Mcp.Core.Extensions;

/// <summary>
/// Registers host-owned <see cref="IEndpointValidator"/> services with protection enabled by default.
/// </summary>
public static class EndpointValidationServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="EndpointValidator"/> as a singleton <see cref="IEndpointValidator"/>
    /// and, if absent, an immutable <see cref="SsrfProtectionPolicy"/> with no overrides.
    /// Shares the host's singleton <see cref="ICommandContextAccessor"/>.
    /// </summary>
    /// <param name="services">The host's service collection.</param>
    /// <returns>
    /// The <paramref name="services"/> collection for chaining.
    /// </returns>
    /// <remarks>
    /// Register a configured <see cref="SsrfProtectionPolicy"/> during host composition to override the default.
    /// Repeated dependency registrations preserve that instance; no process-wide initialization is required.
    /// </remarks>
    public static IServiceCollection AddEndpointValidation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddLogging();
        services.AddCommandContextAccessor();
        services.TryAddSingleton(new SsrfProtectionPolicy(null));
        services.TryAddSingleton<IEndpointValidator, EndpointValidator>();
        return services;
    }
}
