// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore externaldata

using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor.Validation;

/// <summary>
/// Enforces the structural invariants of the server-owned single-table log search contract.
/// This is deliberately not a KQL parser: it only rejects shapes that would break the contract
/// (extra sources, extra statements, nested pipelines). Azure validates query semantics.
/// </summary>
internal static class LogSearchQueryValidator
{
    private const int MaxPipelineLength = 10_000;

    private static readonly HashSet<string> s_unsupportedOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "externaldata",
        "find",
        "invoke",
        "join",
        "search"
    };

    private static readonly HashSet<string> s_sourceFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "app",
        "cluster",
        "resource",
        "table",
        "workspace"
    };

    public static void Validate(string? table, string? pipeline)
    {
        ValidateTableIdentifier(table);
        ValidatePipeline(pipeline);
    }

    public static void ValidateTableIdentifier(string? table)
    {
        if (string.IsNullOrEmpty(table))
        {
            throw new CommandValidationException("--table is required.");
        }

        if (!IsAsciiLetter(table[0]) || table.Any(character => !IsAsciiLetterOrDigitOrUnderscore(character)))
        {
            throw new CommandValidationException(
                "--table must be an ASCII KQL identifier beginning with a letter and containing only letters, digits, or underscores.");
        }
    }

    public static void ValidatePipeline(string? pipeline)
    {
        if (string.IsNullOrWhiteSpace(pipeline))
        {
            throw new CommandValidationException("--query is required.");
        }

        if (pipeline.Length > MaxPipelineLength)
        {
            throw new CommandValidationException(
                $"--query cannot exceed {MaxPipelineLength:N0} characters.");
        }

        var trimmed = pipeline.Trim();
        if (trimmed.Any(character => char.IsControl(character) && character is not ('\t' or '\r' or '\n')))
        {
            throw new CommandValidationException("--query contains a disallowed control character.");
        }

        if (trimmed[0] != '|')
        {
            throw new CommandValidationException("--query must be a KQL pipeline fragment beginning with '|'.");
        }

        ValidateStructure(BlankQuotedTextAndRejectComments(trimmed));
    }

    /// <summary>
    /// Blanks out string literals so structural scanning never sees quoted content, and rejects
    /// comments, which can only be recognized while scanning outside a literal.
    /// Standard literals ('..' / "..") use backslash escapes; verbatim literals (@'..' / @"..")
    /// have no backslash escape and represent an embedded quote by doubling it.
    /// </summary>
    private static string BlankQuotedTextAndRejectComments(string pipeline)
    {
        var result = pipeline.ToCharArray();
        char quote = '\0';
        bool verbatim = false;
        bool escaped = false;

        for (int index = 0; index < pipeline.Length; index++)
        {
            var character = pipeline[index];
            if (quote == '\0')
            {
                if ((character == '/' && index + 1 < pipeline.Length &&
                     pipeline[index + 1] is '/' or '*') ||
                    (character == '*' && index + 1 < pipeline.Length &&
                     pipeline[index + 1] == '/'))
                {
                    throw new CommandValidationException("--query comments are not allowed.");
                }

                if (character is '\'' or '"')
                {
                    quote = character;
                    verbatim = index > 0 && pipeline[index - 1] == '@';
                    result[index] = ' ';
                }

                continue;
            }

            result[index] = ' ';

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (!verbatim && character == '\\')
            {
                escaped = true;
                continue;
            }

            if (character != quote)
            {
                continue;
            }

            if (verbatim && index + 1 < pipeline.Length && pipeline[index + 1] == quote)
            {
                result[++index] = ' ';
                continue;
            }

            quote = '\0';
        }

        if (quote != '\0')
        {
            throw new CommandValidationException("--query contains an unterminated string literal.");
        }

        return new(result);
    }

    private static void ValidateStructure(string pipeline)
    {
        int parenthesisDepth = 0;
        foreach (var character in pipeline)
        {
            if (character == ';')
            {
                throw new CommandValidationException(
                    "--query must contain one pipeline and cannot contain semicolons or multiple statements.");
            }

            if (character == '(')
            {
                parenthesisDepth++;
            }
            else if (character == ')')
            {
                parenthesisDepth--;
                if (parenthesisDepth < 0)
                {
                    throw new CommandValidationException("--query contains unbalanced parentheses.");
                }
            }
            else if (character == '|' && parenthesisDepth > 0)
            {
                throw new CommandValidationException(
                    "--query cannot contain nested tabular pipelines or source expressions.");
            }
        }

        if (parenthesisDepth != 0)
        {
            throw new CommandValidationException("--query contains unbalanced parentheses.");
        }

        var tokens = Tokenize(pipeline);
        for (int index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Equals("let", StringComparison.OrdinalIgnoreCase))
            {
                throw new CommandValidationException("--query cannot contain statement or source rebinding.");
            }

            if (s_unsupportedOperators.Contains(token))
            {
                throw new CommandValidationException(
                    $"The '{token}' operator is not supported for Basic or Auxiliary table searches.");
            }

            if (s_sourceFunctions.Contains(token) &&
                index + 1 < tokens.Count &&
                tokens[index + 1] == "(")
            {
                throw new CommandValidationException(
                    $"The '{token}()' source form is not allowed in a workspace log search.");
            }
        }
    }

    private static List<string> Tokenize(string value)
    {
        var tokens = new List<string>();
        for (int index = 0; index < value.Length;)
        {
            if (IsAsciiLetter(value[index]) || value[index] == '_')
            {
                int start = index++;
                while (index < value.Length && IsAsciiLetterOrDigitOrUnderscore(value[index]))
                {
                    index++;
                }

                tokens.Add(value[start..index]);
            }
            else
            {
                if (!char.IsWhiteSpace(value[index]))
                {
                    tokens.Add(value[index].ToString());
                }

                index++;
            }
        }

        return tokens;
    }

    private static bool IsAsciiLetter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool IsAsciiLetterOrDigitOrUnderscore(char character) =>
        IsAsciiLetter(character) || character is >= '0' and <= '9' or '_';
}
