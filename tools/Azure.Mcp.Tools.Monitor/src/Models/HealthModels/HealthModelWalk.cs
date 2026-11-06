// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>A walk through the dependency graph from one entity.</summary>
public enum HealthModelWalk
{
    /// <summary>Down through the children that drive each state: what drives the entity's state.</summary>
    Causes,

    /// <summary>Everything below the entity.</summary>
    Dependencies,

    /// <summary>Everything above the entity.</summary>
    Dependents,
}
