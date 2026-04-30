// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Overrides the implicit option definition derived from a public property on an options POCO.
/// <para>
/// By convention, every public property on the options class becomes a CLI option:
/// <list type="bullet">
///   <item><b>Name</b>: derived from the property name in kebab-case (e.g., <c>VaultName</c> → <c>--vault-name</c>).</item>
///   <item><b>Required</b>: determined by nullability — non-nullable = required, nullable = optional.</item>
///   <item><b>Type</b>: derived from the property's CLR type.</item>
/// </list>
/// Apply this attribute to override any of these defaults.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class OptionAttribute : Attribute
{
    /// <summary>
    /// Override the CLI option name (without the "--" prefix).
    /// When null, the name is derived from the property name in kebab-case.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// A description of what the option controls. Used in help text and by AI agents.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Override whether the option is required.
    /// When null, requiredness is inferred from the property's nullability.
    /// </summary>
    public bool? Required { get; init; }

    /// <summary>
    /// Whether the option is hidden from help output.
    /// </summary>
    public bool Hidden { get; init; }
}
