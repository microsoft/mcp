// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

public class ConnectionToggleOptions : BaseDocumentDbOptions
{
    public string? Action { get; set; }

    public string? ConnectionString { get; set; }

    public bool TestConnection { get; set; } = true;
}
