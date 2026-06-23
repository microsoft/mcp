// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Options;

public interface IClusterOption
{
    string? Cluster { get; }
    string? ClusterUri { get; }
}
