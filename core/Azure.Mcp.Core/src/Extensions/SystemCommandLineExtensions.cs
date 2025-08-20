// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Azure.Mcp.Core.Extensions;

/// <summary>
/// Extension methods for System.CommandLine types
/// </summary>
public static class SystemCommandLineExtensions
{
    /// <summary>
    /// Gets the default value from an option using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="option">The option to get the default value from.</param>
    /// <returns>The default value as a string, or an empty string if not found.</returns>
    public static T GetDefaultValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicProperties)] T>(this Option<T> option)
    {
        try
        {
            var argumentProperty = typeof(T).GetProperty("Argument", BindingFlags.Instance | BindingFlags.NonPublic);
            var argument = argumentProperty?.GetValue(option);
            if (argument != null)
            {
                var getDefaultValueMethod = typeof(Argument<T>).GetMethod("GetDefaultValue");
                if (getDefaultValueMethod != null)
                {
                    var defaultValue = getDefaultValueMethod.Invoke(argument, null);
                    return (T)(defaultValue ?? default!);
                }
            }
            return default!;
        }
        catch
        {
            // Silently handle any reflection or default value retrieval errors
        }

        return default!;
    }
}
