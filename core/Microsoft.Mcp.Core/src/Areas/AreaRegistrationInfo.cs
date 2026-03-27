// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Microsoft.Mcp.Core.Areas;

/// <summary>
/// Non-generic wrapper that captures the static members of an <see cref="IAreaRegistration"/>
/// implementation so they can be used polymorphically at runtime.
/// <para>
/// Use <see cref="AreaRegistrationInfo.Create{T}"/> to create instances from concrete types.
/// </para>
/// </summary>
public sealed class AreaRegistrationInfo(
    string areaName,
    string areaTitle,
    CommandCategory category,
    Func<CommandGroupDescriptor> getCommandDescriptors,
    Action<IServiceCollection> registerServices,
    Func<string, IServiceProvider, IBaseCommand> resolveHandler)
{
    /// <summary>
    /// The unique area name.
    /// </summary>
    public string AreaName { get; } = areaName;

    /// <summary>
    /// The user-friendly title.
    /// </summary>
    public string AreaTitle { get; } = areaTitle;

    /// <summary>
    /// The command category.
    /// </summary>
    public CommandCategory Category { get; } = category;

    /// <summary>
    /// Returns the command group descriptor tree for this area.
    /// </summary>
    public CommandGroupDescriptor GetCommandDescriptors() => getCommandDescriptors();

    /// <summary>
    /// Registers handler types and service dependencies into DI.
    /// </summary>
    public void RegisterServices(IServiceCollection services) => registerServices(services);

    /// <summary>
    /// Resolves an <see cref="IBaseCommand"/> by its handler type name from DI.
    /// This is trim-safe because the concrete type mapping is captured at registration time.
    /// </summary>
    public IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider)
        => resolveHandler(handlerTypeName, serviceProvider);

    /// <summary>
    /// Creates an <see cref="AreaRegistrationInfo"/> from a concrete <see cref="IAreaRegistration"/> type.
    /// This captures the static abstract members as delegates for polymorphic use.
    /// </summary>
    public static AreaRegistrationInfo Create<T>() where T : IAreaRegistration
    {
        return new AreaRegistrationInfo(
            T.AreaName,
            T.AreaTitle,
            T.Category,
            T.GetCommandDescriptors,
            T.RegisterServices,
            T.ResolveHandler);
    }
}
