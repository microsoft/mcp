// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Derives CLI option names from C# property names using kebab-case convention.
/// Splits on PascalCase word boundaries and lowercases the result.
/// Use <see cref="OptionAttribute.Name"/> to override when the convention doesn't produce the desired name.
/// <example>
///   <c>VaultName</c> → <c>vault-name</c>,
///   <c>ResourceGroup</c> → <c>resource-group</c>,
///   <c>Subscription</c> → <c>subscription</c>,
///   <c>MaxRetries</c> → <c>max-retries</c>,
///   <c>HTTPSOnly</c> → <c>https-only</c>.
/// </example>
/// </summary>
public static class OptionNameConvention
{
    /// <summary>
    /// Converts a PascalCase property name to a kebab-case option name (without "--" prefix).
    /// </summary>
    public static string ToKebabCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        var sb = new StringBuilder(propertyName.Length + 4);

        for (int i = 0; i < propertyName.Length; i++)
        {
            char c = propertyName[i];

            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(propertyName[i - 1]))
                {
                    // camelCase boundary: "vaultName" → "vault-n"
                    sb.Append('-');
                }
                else if (i > 0 && i + 1 < propertyName.Length
                    && char.IsUpper(propertyName[i - 1]) && char.IsLower(propertyName[i + 1]))
                {
                    // Acronym-to-word boundary: "HTTPSOnly" → "https-only"
                    sb.Append('-');
                }

                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the full CLI option flag (e.g., "--vault-name") for a property name.
    /// </summary>
    public static string ToOptionFlag(string propertyName) =>
        $"--{ToKebabCase(propertyName)}";
}
