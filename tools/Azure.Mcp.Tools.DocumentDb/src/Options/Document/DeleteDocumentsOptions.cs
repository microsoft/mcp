// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

public sealed class DeleteDocumentsOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }

    public string? CollectionName { get; set; }

    public string? Filter { get; set; }

    public string? Mode { get; set; }
}
