// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands;

/// <summary>
/// The fixed reply format every Azure Catalyst tool result asks the agent to use, so replies are short and look the same across
/// tools: one headline line, at most four bullets with exact figures and the issues found, and one 'Next step:' line that moves the
/// user along the migration journey (inventory, then business case and ROI summary, then migration planning) or tells them how to
/// fix an error. Progress messages while an operation runs are not replies and keep their own one-line rule.
/// </summary>
internal static class ReplyFormat
{
    internal const string Rules =
        "Reply to the user in exactly this format and nothing else: " +
        "(1) one headline line in bold; " +
        "(2) at most 4 short bullets with exact figures and any issues found, taken from this result without inventing details " +
        "(fewer bullets when there are fewer facts; leave out a figure the result does not give, never say it is missing); " +
        "(3) one line starting with 'Next step:'. " +
        "A link or code block asked for below goes inside a bullet or right after the bullets. No greeting, preamble, recap of " +
        "earlier messages, closing questions or extra offers, and no tool or parameter names.";

    /// <summary>Journey step after an inventory is ready.</summary>
    internal const string AfterInventory =
        "offer to create the business case and ROI summary from this inventory, asking for their target Azure region if they have " +
        "not given it.";

    /// <summary>Journey step after a business case is ready.</summary>
    internal const string AfterBusinessCase =
        "share the ROI report with stakeholders, or change assumptions (for example discount, savings option or Azure Hybrid " +
        "Benefit) and create the business case again; then plan the migration waves for the servers that save the most.";

    /// <summary>Returns the reply instructions with the content of the headline, the bullets and the next step.</summary>
    internal static string Reply(string headline, string bullets, string nextStep) =>
        $"{Rules} Headline: {headline} Bullets: {bullets} Next step: {nextStep}";

    /// <summary>Returns the reply instructions for an error: the cause from the result, then how to fix it and carry on.</summary>
    internal static string Failure(string headline, string nextStep) =>
        Reply(headline, "the cause from 'errorMessage' or 'error' in plain language, quoting any figures or names exactly, and what " +
            "in the user's input or file causes it when the result says so.", nextStep);
}
