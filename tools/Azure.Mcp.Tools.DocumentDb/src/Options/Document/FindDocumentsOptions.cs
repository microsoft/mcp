// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

public class FindDocumentsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }

    public string? CollectionName { get; set; }

    public string? Filter { get; set; }

    public string? Options { get; set; }
}
