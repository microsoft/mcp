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
    [DynamicallyAccessedMembers(OptionContainerAttribute.ContainerMembers)] TContainer> : OptionContainerAttribute
{
    [DynamicallyAccessedMembers(ContainerMembers)]
    internal override Type ContainerType => typeof(TContainer);
}
