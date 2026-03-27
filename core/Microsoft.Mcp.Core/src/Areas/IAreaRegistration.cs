// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Microsoft.Mcp.Core.Areas;

/// <summary>
/// Defines a command area with static-only registration. Command metadata is provided
/// via <see cref="GetCommandDescriptors"/> without constructing any handler instances,
/// and service registration is separate from command discovery.
/// <para>
/// This interface replaces <see cref="IAreaSetup"/> and is designed so that command
/// descriptors can be serialized to a manifest cache at build time, enabling CLI
/// parsing without loading tool assemblies at runtime.
/// </para>
/// </summary>
public interface IAreaRegistration
{
    /// <summary>
    /// Gets the unique area name used as the CLI command group verb (e.g. "storage", "keyvault").
    /// </summary>
    static abstract string AreaName { get; }

    /// <summary>
    /// Gets the user-friendly title of the area for display purposes.
    /// </summary>
    static abstract string AreaTitle { get; }

    /// <summary>
    /// Gets the category the area's commands belong to.
    /// </summary>
    static abstract CommandCategory Category { get; }

    /// <summary>
    /// Returns the full command tree for this area as serializable descriptors.
    /// This method must not depend on DI or any runtime state — it is pure metadata.
    /// </summary>
    static abstract CommandGroupDescriptor GetCommandDescriptors();

    /// <summary>
    /// Registers command handler types and their service dependencies into the DI container.
    /// Called only when commands from this area will actually be executed.
    /// </summary>
    static abstract void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Resolves a command handler by its type name from the service provider.
    /// This method provides trim-safe handler resolution because the concrete type
    /// mapping is known statically in each area's implementation.
    /// </summary>
    /// <param name="handlerTypeName">The handler type name from <see cref="Descriptors.CommandDescriptor.HandlerType"/>.</param>
    /// <param name="serviceProvider">The DI service provider.</param>
    static abstract IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider);
}
