// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>What an inspection adds to each entity it returns.</summary>
public enum HealthModelInclude
{
    Signals,
    History,
    SignalHistory,
    Annotations,
}
