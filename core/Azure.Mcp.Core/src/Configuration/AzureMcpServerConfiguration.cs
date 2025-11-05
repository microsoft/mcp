// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Configuration;

public class AzureMcpServerConfiguration
{
    public const string DefaultName = "Azure.Mcp.Server";

    public string Name { get; set; } = DefaultName;

    public string Version { get; set; } = "1.0.0-beta";

    public bool IsTelemetryEnabled { get; set; } = true;
}
