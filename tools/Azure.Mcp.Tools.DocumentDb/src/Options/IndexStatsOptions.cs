// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

public class IndexStatsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }

    public string? CollectionName { get; set; }
}
