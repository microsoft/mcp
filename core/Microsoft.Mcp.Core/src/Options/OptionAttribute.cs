// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Options;

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
    /// Additional CLI option names (without the "--" prefix).
    /// </summary>
    public string[]? Aliases { get; init; }

    /// <summary>
    /// A description of what the option controls. Used in help text and by AI agents.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// A default value for the option when a value is not provided. Must match the property type being attributed.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Whether the option is hidden from help output. Default is false.
    /// </summary>
    public bool Hidden { get; init; } = false;

    /// <summary>
    /// Whether the option allows an empty or whitespace-only string as a valid value. Default handling is to reject such values.
    /// </summary>
    public bool AllowEmptyOrWhiteSpaceString { get; init; } = false;
}
