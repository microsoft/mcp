// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Search.Options.Knowledge;

public sealed class KnowledgeBaseRetrieveOptions
{
    [Option(Description = SearchOptionDescriptions.KnowledgeBase)]
    public required string KnowledgeBase { get; set; }

    [Option(Description = "Natural language query for retrieval when a conversational message history isn't provided.")]
    public string? Query { get; set; }

    [Option(Description = "Conversation history messages passed to the knowledge base. Able to specify multiple --messages entries. Each entry formatted as role:content, where role is `user` or `assistant` (e.g., user:How many docs?).")]
    public string[]? Messages { get; set; }

    [Option(Description = SearchOptionDescriptions.Service)]
    public required string Service { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
