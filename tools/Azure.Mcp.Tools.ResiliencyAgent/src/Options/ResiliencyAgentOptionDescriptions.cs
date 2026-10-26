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
        "The user's request about Azure resilience, in their own words. Include Azure resource IDs, " +
        "resource types, regions, SKUs, and architecture facts explicitly stated in the conversation. " +
        "Do not copy content from attached, referenced, or workspace files into this field. When the " +
        "selected command supports file inputs, prepare those files with resiliencyagent_file_attach and " +
        "pass the returned attachment IDs.";

    public const string ConversationId =
        "The conversationId returned by a previous call, to continue that conversation. Omit it to start " +
        "a new one. Pass it back whenever the user is following up on the same topic in this chat.";

    public const string AttachmentIds =
        "Opaque attachment IDs returned by resiliencyagent_file_attach. Include every file required for " +
        "this request in one call. The request fails if any ID is unknown, expired, duplicated beyond the " +
        "supported limits, or cannot be resolved; files are never silently omitted.";
}
