// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ToolMetadataExporter.Models;
using ToolMetadataExporter.Models.Kusto;

namespace ToolMetadataExporter.Services;

public interface IAzureMcpDatastore
{
    Task<IList<AzureMcpTool>> GetAvailableToolsAsync(CancellationToken cancellationToken = default);

    Task AddToolEventsAsync(IList<McpToolEvent> toolEvents, CancellationToken cancellationToken = default);
}
