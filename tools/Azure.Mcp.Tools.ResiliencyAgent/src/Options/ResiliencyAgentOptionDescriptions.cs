// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Options;

/// <summary>
/// Option descriptions shared across the Resiliency Agent commands.
/// </summary>
/// <remarks>
/// These strings are the only guidance that reaches every caller, including users who have the Azure
/// MCP server without our plugin. They are written to be prescriptive rather than merely descriptive.
/// </remarks>
public static class ResiliencyAgentOptionDescriptions
{
    public const string Request =
        "The user's request about Azure resilience, in their own words. Include any Azure resource ids, " +
        "resource types, regions, SKUs or architecture details already visible in the conversation or in " +
        "open files - the agent identifies resources from this text.";

    public const string ConversationId =
        "The conversationId returned by a previous call, to continue that conversation. Omit it to start " +
        "a new one. Pass it back whenever the user is following up on the same topic in this chat.";
}
