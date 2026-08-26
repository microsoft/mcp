// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Identifies a complex option container and preserves the members required to discover and bind its options.
/// </summary>
/// <typeparam name="TContainer">The type of the option container.</typeparam>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class OptionContainerAttribute<
    [DynamicallyAccessedMembers(ContainerMembers)] TContainer> : Attribute, IOptionContainerMetadata
{
    private const DynamicallyAccessedMemberTypes ContainerMembers =
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

    /// <summary>
    /// The prefix to use for the options in the container.
    /// If null, the property name (in kebab-case) is used as the prefix.
    /// </summary>
    public string? Prefix { get; init; }

    [DynamicallyAccessedMembers(ContainerMembers)]
    Type IOptionContainerMetadata.ContainerType => typeof(TContainer);
}
