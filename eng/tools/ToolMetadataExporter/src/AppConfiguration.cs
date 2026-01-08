// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ToolMetadataExporter;

public class AppConfiguration
{
    public string? IngestionEndpoint { get; set; }

    public string? QueryEndpoint { get; set; }

    public string? DatabaseName { get; set; }

    public string? McpToolEventsTableName { get; set; }

    public string? QueriesFolder { get; set; } = "Resources/queries";

    public string? WorkDirectory { get; set; }

    public bool IsDryRun { get; set; }

    public string? AzmcpExe { get; set; }

    public bool IsAzmcpExeSpecified { get; set; }
}
