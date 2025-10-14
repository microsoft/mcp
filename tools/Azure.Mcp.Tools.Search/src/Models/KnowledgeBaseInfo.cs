// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Search.Models;

public sealed record KnowledgeBaseInfo(string Name, string Description, List<string> KnowledgeSources);
