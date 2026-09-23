// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Extensions for projecting <see cref="Option"/> definitions into serializable metadata models.
/// </summary>
internal static class OptionExtensions
{
    /// <summary>
    /// Creates an <see cref="OptionInfo"/> describing the option for CLI tool metadata
    /// (surfaced by <c>tools list</c> and <c>--learn</c>), including the JSON Schema type keyword
    /// derived from the option's value type.
    /// </summary>
    /// <param name="option">The command-line option to describe.</param>
    /// <returns>An <see cref="OptionInfo"/> describing the option.</returns>
    public static OptionInfo ToOptionInfo(this Option option)
    {
        ArgumentNullException.ThrowIfNull(option);

        return new OptionInfo(
            name: option.Name,
            description: option.Description ?? string.Empty,
            type: OptionTypeNameHelper.GetJsonSchemaType(option.ValueType),
            required: option.Required);
    }
}
