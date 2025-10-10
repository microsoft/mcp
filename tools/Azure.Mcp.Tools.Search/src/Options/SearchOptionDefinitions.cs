// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Options;

public static class SearchOptionDefinitions
{
    public const string ServiceName = "service";
    public const string IndexName = "index";
    public const string QueryName = "query";
    public const string KnowledgeBaseName = "knowledge-base";
    public const string KnowledgeSourceName = "knowledge-source";
    public const string MessagesName = "messages";

    public static readonly Option<string> Service = new(
        $"--{ServiceName}"
    )
    {
        Description = "The name of the Azure AI Search service (e.g., my-search-service).",
        Required = true
    };

    public static readonly Option<string> Index = new(
        $"--{IndexName}"
    )
    {
        Description = "The name of the search index within the Azure AI Search service.",
        Required = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryName}"
    )
    {
        Description = "The search query to execute against the Azure AI Search index.",
        Required = true
    };

    public static readonly Option<string> KnowledgeBase = new(
        $"--{KnowledgeBaseName}"
    )
    {
        Description = "The name of the knowledge base within the Azure AI Search service.",
        Required = true
    };

    public static readonly Option<string> KnowledgeSource = new(
        $"--{KnowledgeSourceName}"
    )
    {
        Description = "The name of the knowledge source within the Azure AI Search service.",
        Required = true
    };

    public static readonly Option<string> KnowledgeQuery = new(
        $"--{QueryName}"
    )
    {
        Description = "Natural language query for retrieval when a conversational message history isn't provided.",
        Required = false
    };

    public static readonly Option<string[]> Messages = new(
        $"--{MessagesName}")
    {
        Description = "Conversation history messages passed to the knowledge base. Able to specify multiple --messages entries. Each entry formatted as role:content, where role is `user` or `assistant` (e.g., user:How many docs?).",
        Arity = ArgumentArity.ZeroOrMore,
        AllowMultipleArgumentsPerToken = true
    };
}
