// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>
/// An entity a walk reached and the step it was reached from. The path is built only when read, so a walk keeps one
/// link per entity instead of a path per entity.
/// </summary>
internal sealed record HealthModelWalkStep(HealthModelWalkStep? From, string Name, JsonElement Entity)
{
    /// <summary>The entity names from the walk's start to this entity, joined by '/'.</summary>
    public string Path
    {
        get
        {
            var names = new List<string>();
            for (var step = this; step is not null; step = step.From)
            {
                names.Add(step.Name);
            }
            names.Reverse();
            return string.Join('/', names);
        }
    }
}
