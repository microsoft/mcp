// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

public class FindAndModifyOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }

    public string? CollectionName { get; set; }

    public string? Filter { get; set; }

    public string? Update { get; set; }

    public bool Upsert { get; set; }
}
