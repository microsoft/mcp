// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

public sealed class GetStatsOptions : BaseDocumentDbOptions
{
    public string? ResourceType { get; set; }

    public string? DbName { get; set; }

    public string? CollectionName { get; set; }
}