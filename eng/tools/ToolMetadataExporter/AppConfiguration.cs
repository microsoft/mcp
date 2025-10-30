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
}
