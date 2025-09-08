// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Models;

public sealed record KnowledgeAgentInfo(string Name, string Description, List<string> KnowledgeSources);
