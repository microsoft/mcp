// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Microsoft.Mcp.Core.Areas;

/// <summary>
/// Base class for area registrations. Subclass this to define a concrete area
/// (e.g. <c>StorageSetup</c>) that describes its commands and service registrations.
/// </summary>
public class AreaRegistrationInfo
{
    /// <summary>
    /// Gets the full command tree for this area as serializable descriptors.
    /// The descriptor's <see cref="CommandGroupDescriptor.Name"/>,
    /// <see cref="CommandGroupDescriptor.Title"/>, and
    /// <see cref="CommandGroupDescriptor.Category"/> provide the area identity.
    /// </summary>
    public virtual CommandGroupDescriptor CommandDescriptors => throw new NotImplementedException();

    /// <summary>
    /// Registers command handler types and their service dependencies into the DI container.
    /// Override in subclasses to provide service registrations.
    /// </summary>
    public virtual void ConfigureServices(IServiceCollection services) { }

    /// <summary>
    /// Optional pre-built command group that bypasses the descriptor build path.
    /// Used by consolidated tool discovery where commands are already resolved.
    /// </summary>
    public CommandGroup? CommandGroupOverride { get; init; }

    /// <summary>
    /// Registers a command handler as a keyed singleton scoped to this area.
    /// The key is <c>"CommandDescriptors.Name:typeof(T).Name"</c>, matching the
    /// <see cref="CommandDescriptor.HandlerType"/> used in descriptors.
    /// </summary>
    /// <summary>
    /// Builds the keyed service key for a command handler, scoped to an area.
    /// Format: <c>"areaName:handlerTypeName"</c>.
    /// </summary>
    internal static string GetCommandKey(string areaName, string handlerTypeName) => $"{areaName}:{handlerTypeName}";

    /// <summary>
    /// Registers a command handler as a keyed singleton scoped to this area.
    /// The key is <c>"CommandDescriptors.Name:typeof(T).Name"</c>, matching the
    /// <see cref="CommandDescriptor.HandlerType"/> used in descriptors.
    /// </summary>
    protected IServiceCollection AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        IServiceCollection services) where T : class, IBaseCommand
    {
        services.AddKeyedSingleton<IBaseCommand, T>(GetCommandKey(CommandDescriptors.Name, typeof(T).Name));
        return services;
    }
}
