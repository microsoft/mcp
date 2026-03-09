// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DocumentDb.Options;

// Connection Options
public class ConnectDocumentDbOptions : BaseDocumentDbOptions
{
    public string? ConnectionString { get; set; }
    public bool TestConnection { get; set; } = true;
}

public class DisconnectDocumentDbOptions : BaseDocumentDbOptions;

public class GetConnectionStatusOptions : BaseDocumentDbOptions;
