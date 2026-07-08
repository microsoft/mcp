// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Provides deterministic (non-sampling) tool and command resolution using lexical matching.
/// Phase 3 Workstream E: Sampling deprecation and deterministic routing.
/// This class serves as a fallback when sampling is unavailable or deprecated.
/// </summary>
public static class DeterministicToolResolution
{
    /// <summary>
    /// Attempts to match a tool name from a list based on intent text.
    /// Uses lexical similarity and keyword matching as a deterministic fallback.
    /// </summary>
    /// <param name="intent">The user's intent text.</param>
    /// <param name="toolsJson">JSON representation of available tools.</param>
    /// <returns>Tuple of (matched tool name, confidence score 0-1).</returns>
    public static (string? toolName, float confidence) MatchToolFromIntent(string intent, string toolsJson)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return (null, 0f);
        }

        try
        {
            // Parse tools JSON to extract tool names
            var toolsNode = JsonNode.Parse(toolsJson);
            if (toolsNode is not JsonArray toolsArray)
            {
                return (null, 0f);
            }

            var intentLower = intent.ToLowerInvariant();
            var bestMatch = (toolName: (string?)null, score: 0f);

            foreach (var toolNode in toolsArray.OfType<JsonObject>())
            {
                if (toolNode["name"] is JsonValue nameNode && nameNode.GetValue<string>() is string toolName)
                {
                    var toolNameLower = toolName.ToLowerInvariant();
                    var score = CalculateSimilarity(intentLower, toolNameLower);

                    // Also check description for keyword matches
                    if (toolNode["description"] is JsonValue descNode && descNode.GetValue<string>() is string description)
                    {
                        var descScore = CalculateSimilarity(intentLower, description.ToLowerInvariant());
                        score = Math.Max(score, descScore * 0.8f); // Description match weighted slightly lower
                    }

                    if (score > bestMatch.score)
                    {
                        bestMatch = (toolName, score);
                    }
                }
            }

            return bestMatch.score > 0.3f ? bestMatch : (null, 0f);
        }
        catch
        {
            return (null, 0f);
        }
    }

    /// <summary>
    /// Attempts to match a command name from available tools based on intent text.
    /// Uses lexical similarity as a deterministic fallback.
    /// </summary>
    /// <param name="intent">The user's intent text.</param>
    /// <param name="toolName">The specific tool context.</param>
    /// <param name="toolsJson">JSON representation of available tools and commands.</param>
    /// <returns>Tuple of (matched command name, confidence score 0-1).</returns>
    public static (string? commandName, float confidence) MatchCommandFromIntent(string intent, string toolName, string toolsJson)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return (null, 0f);
        }

        try
        {
            var toolsNode = JsonNode.Parse(toolsJson);
            if (toolsNode is not JsonArray toolsArray)
            {
                return (null, 0f);
            }

            var intentLower = intent.ToLowerInvariant();
            var bestMatch = (commandName: (string?)null, score: 0f);

            foreach (var toolNode in toolsArray.OfType<JsonObject>())
            {
                if (toolNode["name"] is JsonValue toolNameNode && toolNameNode.GetValue<string>() is string currentToolName)
                {
                    if (string.Equals(currentToolName, toolName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Found the correct tool, now match commands
                        if (toolNode["commands"] is JsonArray commandsArray)
                        {
                            foreach (var cmdNode in commandsArray.OfType<JsonObject>())
                            {
                                if (cmdNode["name"] is JsonValue cmdNameNode && cmdNameNode.GetValue<string>() is string commandName)
                                {
                                    var commandNameLower = commandName.ToLowerInvariant();
                                    var score = CalculateSimilarity(intentLower, commandNameLower);

                                    // Also check command description
                                    if (cmdNode["description"] is JsonValue descNode && descNode.GetValue<string>() is string description)
                                    {
                                        var descScore = CalculateSimilarity(intentLower, description.ToLowerInvariant());
                                        score = Math.Max(score, descScore * 0.8f);
                                    }

                                    if (score > bestMatch.score)
                                    {
                                        bestMatch = (commandName, score);
                                    }
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return bestMatch.score > 0.3f ? bestMatch : (null, 0f);
        }
        catch
        {
            return (null, 0f);
        }
    }

    /// <summary>
    /// Calculates similarity between two strings using a simple Levenshtein-like distance metric.
    /// Returns a score between 0 and 1 where 1 is an exact match.
    /// </summary>
    private static float CalculateSimilarity(string text1, string text2)
    {
        // Exact match
        if (text1 == text2)
        {
            return 1f;
        }

        // Token-based matching
        var tokens1 = TokenizeText(text1);
        var tokens2 = TokenizeText(text2);

        if (tokens1.Count == 0 || tokens2.Count == 0)
        {
            return 0f;
        }

        // Count matching tokens
        var matchingTokens = tokens1.Intersect(tokens2, StringComparer.OrdinalIgnoreCase).Count();
        var totalTokens = Math.Max(tokens1.Count, tokens2.Count);

        var tokenScore = (float)matchingTokens / totalTokens;

        // Levenshtein distance for single-token comparison
        if (tokens1.Count == 1 && tokens2.Count == 1)
        {
            var distance = LevenshteinDistance(tokens1[0], tokens2[0]);
            var maxLen = Math.Max(tokens1[0].Length, tokens2[0].Length);
            var distanceScore = 1f - ((float)distance / maxLen);
            return (tokenScore + distanceScore) / 2f;
        }

        return tokenScore;
    }

    /// <summary>
    /// Tokenizes text into words for comparison.
    /// </summary>
    private static List<string> TokenizeText(string text)
    {
        return text.Split(new[] { ' ', '_', '-', '/', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 0)
            .ToList();
    }

    /// <summary>
    /// Calculates Levenshtein distance between two strings.
    /// </summary>
    private static int LevenshteinDistance(string s1, string s2)
    {
        if (s1.Length == 0)
            return s2.Length;
        if (s2.Length == 0)
            return s1.Length;

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }
}
