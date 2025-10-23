// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ToolMetadataExporter;

public class AppConfiguration
{
    public string? ClusterEndpoint { get; set; }

    public string? DatabaseName { get; set; }

    public string? McpToolEventsTableName { get; set; }

    public string? QueriesFolder { get; set; }

    public string? WorkDirectory { get; set; }
}
