// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Attribute for complex option containers (aka, classes that are used in option definitions that contain options themselves).
/// <para>
/// Properties with this attribute must be complex types that contain other options.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class OptionContainerAttribute : Attribute
{
    /// <summary>
    /// The prefix to use for the options in the container.
    /// If null, the property name (in kebab-case) is used as the prefix.
    /// <para>
    /// For example, if the property is named "Vault" and the prefix is "v", the options in the container will be prefixed with "--v-".
    /// If the property is named "Vault" and the prefix is null, the options in the container will be prefixed with "--vault-".
    /// </para>
    /// </summary>
    public string? Prefix { get; init; }
}
