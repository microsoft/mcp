// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.BicepSchema.Services.ResourceProperties.Helpers;
public class ApiVersionComparer : IComparer<string>
{
    /// <summary>
    /// Gets the instance.
    /// </summary>
    public static ApiVersionComparer Instance { get; } = new ApiVersionComparer();

    /// <summary>
    /// Prevents a default instance of the <see cref="ApiVersionComparer"/> class from being created.
    /// </summary>
    private ApiVersionComparer()
    {
    }

    /// <summary>
    /// Compares two strings and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first string to compare.</param>
    /// <param name="y">The second string to compare.</param>
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null || x.Length == y.Length)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(x, y);
        }

        if (x.Length < y.Length)
        {
            int compare = StringComparer.OrdinalIgnoreCase.Compare(x, y.Substring(0, x.Length));

            return compare != 0 ? compare : 1;
        }
        else
        {
            int compare = StringComparer.OrdinalIgnoreCase.Compare(x.Substring(0, y.Length), y);

            return compare != 0 ? compare : -1;
        }
    }
}
