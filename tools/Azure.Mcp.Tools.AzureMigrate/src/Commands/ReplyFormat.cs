// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Commands;

/// <summary>
/// The fixed reply format every Azure Catalyst tool result asks the agent to use, so replies are short and look the same across
/// tools: a hard limit of 3 short lines (a headline, one details line with exact figures or the issue found, and one 'Next step:'
/// line that moves the user along the migration journey or tells them how to fix an error). Progress messages while an operation
/// runs are not replies and keep their own one-line rule.
/// </summary>
internal static class ReplyFormat
{
    internal const string Rules =
        "HARD LIMIT: reply to the user in at most 3 short lines of text in total, and nothing else: " +
        "line 1 a headline in bold; " +
        "line 2 (only if needed) one sentence of at most 25 words with the key exact figures or the issue found, taken from this " +
        "result without inventing details (leave out a figure the result does not give, never say it is missing); " +
        "line 3 starting with '**Next step:**' in bold. Put each line in its own paragraph, separated by a blank line, so the " +
        "'**Next step:**' line always starts on a new line. " +
        "Never use tables, bullet lists, headings or extra paragraphs, and never repeat or re-list what a card above already " +
        "shows (servers, VMs, costs or report sections). A link asked for below goes inside line 2; only a code block explicitly " +
        "asked for below may follow the 3 lines. No greeting, preamble, recap of earlier messages, closing questions or extra " +
        "offers, and no tool or parameter names. Check the reply before sending: if it is longer than 3 lines, shorten it.";

    /// <summary>Reply-length rule added to every Azure Catalyst tool description, so it also covers replies Copilot writes on its
    /// own (for example after a wait gives up or a tool fails), not only replies that follow a tool result.</summary>
    internal const string ToolRule =
        "HARD LIMIT for every reply to the user about Azure Catalyst, including after a timeout or error: at most 3 short lines " +
        "(a bold headline, one short sentence of key facts, and a '**Next step:**' line in bold on its own new line, each separated " +
        "by a blank line). No tables, bullet lists, headings or extra " +
        "paragraphs, and never re-list servers, files or report content.";

    /// <summary>Journey step after an inventory is ready.</summary>
    internal const string AfterInventory =
        "if the user has already asked in this conversation for a business case or an ROI summary, create it from this inventory " +
        "without asking them to confirm: if they have not given a target Azure region, after the reply ask for it with your " +
        "question tool that shows choices (a dropdown), offering common regions such as Central India, East US, West Europe, " +
        "UK South, Southeast Asia and Australia East, not in text. Otherwise only offer, in one line, to create the business case " +
        "and ROI summary from this inventory, and do not ask for the region or create the business case until they ask for it.";

    /// <summary>How to ask the user for a missing target region.</summary>
    internal const string AskRegion =
        "Ask for a missing target region with your question tool that shows choices (a dropdown), offering common regions such as " +
        "Central India, East US, West Europe, UK South, Southeast Asia and Australia East, not in text. ";

    /// <summary>Journey step after a business case is ready.</summary>
    internal const string AfterBusinessCase =
        "share the ROI report with stakeholders, or change assumptions (for example discount, savings option or Azure Hybrid " +
        "Benefit) and create the business case again; then plan the migration waves for the servers that save the most.";

    /// <summary>Returns the reply instructions with the content of the headline, the details line and the next step.</summary>
    internal static string Reply(string headline, string details, string nextStep) =>
        $"{Rules} Headline: {headline} Details line (pick only what fits in one short sentence): {details} Next step: {nextStep}";

    /// <summary>Returns the reply instructions for an error: the cause from the result, then how to fix it and carry on.</summary>
    internal static string Failure(string headline, string nextStep) =>
        Reply(headline, "the cause from 'errorMessage' or 'error' in plain language, quoting any figures or names exactly, and what " +
            "in the user's input or file causes it when the result says so.", nextStep);
}
