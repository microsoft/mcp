// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace McpToolEvaluator.Core.Models;

public static class PromptInteractionExtensions
{
    public const string None = "none";
    public const string ClarificationRequired = "clarification-required";
    public const string ContextRequired = "context-required";
    public const string InvestigationRequired = "investigation-required";

    public static PromptInteraction Parse(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            None => PromptInteraction.None,
            ClarificationRequired => PromptInteraction.ClarificationRequired,
            ContextRequired => PromptInteraction.ContextRequired,
            InvestigationRequired => PromptInteraction.InvestigationRequired,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown prompt interaction.")
        };
    }

    public static string ToDisplayString(this PromptInteraction interaction)
    {
        return interaction switch
        {
            PromptInteraction.None => None,
            PromptInteraction.ClarificationRequired => ClarificationRequired,
            PromptInteraction.ContextRequired => ContextRequired,
            PromptInteraction.InvestigationRequired => InvestigationRequired,
            _ => throw new ArgumentOutOfRangeException(nameof(interaction), interaction, "Unknown prompt interaction.")
        };
    }
}
