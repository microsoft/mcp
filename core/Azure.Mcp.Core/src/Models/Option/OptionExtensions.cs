// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Models.Option;

/// <summary>
/// Extension methods for working with option definitions and command registration.
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Creates a required version of an option definition.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="definition">The option definition.</param>
    /// <returns>A new Option instance that is required.</returns>
    public static Option<T> AsRequired<T>(this OptionDefinition<T> definition) where T : notnull
    {
        definition.Required = true;
        return definition.ToOption();
    }

    /// <summary>
    /// Creates an optional version of an option definition.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="definition">The option definition.</param>
    /// <returns>A new Option instance that is optional.</returns>
    public static Option<T> AsOptional<T>(this OptionDefinition<T> definition) where T : notnull
    {
        definition.Required = false;
        return definition.ToOption();
    }

    /// <summary>
    /// Creates an option with a custom description.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="definition">The option definition.</param>
    /// <param name="description">The custom description.</param>
    /// <param name="required">Whether the option is required.</param>
    /// <returns>A new Option instance with the custom description.</returns>
    public static Option<T> WithDescription<T>(this OptionDefinition<T> definition, string description, bool required = false) where T : notnull
    {
        definition.Description = description;
        definition.Required = required;
        return definition.ToOption();
    }

    /// <summary>
    /// Creates a required version of an existing Option.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="option">The existing option.</param>
    /// <returns>A new Option instance that is required.</returns>
    public static Option<T> AsRequired<T>(this Option<T> option)
    {
        var newOption = new Option<T>(option.Name)
        {
            Description = option.Description,
            Required = true
        };
        return newOption;
    }

    /// <summary>
    /// Creates an optional version of an existing Option.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="option">The existing option.</param>
    /// <returns>A new Option instance that is optional.</returns>
    public static Option<T> AsOptional<T>(this Option<T> option)
    {
        var newOption = new Option<T>(option.Name)
        {
            Description = option.Description,
            Required = false
        };
        return newOption;
    }
}
