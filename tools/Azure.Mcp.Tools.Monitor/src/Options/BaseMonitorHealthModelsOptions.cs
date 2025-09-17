// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options;

public class BaseMonitorHealthModelsOptions : BaseMonitorOptions
{
    public string? Entity { get; set; }
    public string? HealthModelName { get; set; }
}
