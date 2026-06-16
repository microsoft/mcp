// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

public sealed class ConnectorTestResult
{
    public string? ConnectorName { get; set; }

    public bool Success { get; set; }

    public List<ConnectorToolInfo>? Tools { get; set; }

    public int TotalCount { get; set; }

    public string? ErrorMessage { get; set; }

    public int? ResponseTimeMs { get; set; }
}
