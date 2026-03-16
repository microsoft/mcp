// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

public sealed class ExplainQueryOptions : BaseDocumentDbOptions
{
    public string? DbName { get; set; }

    public string? CollectionName { get; set; }

    public string? Operation { get; set; }

    public string? QueryBody { get; set; }
}
