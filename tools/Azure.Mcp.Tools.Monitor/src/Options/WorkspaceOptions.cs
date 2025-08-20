// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Options
{
    public abstract class WorkspaceOptions : BaseMonitorOptions, IWorkspaceOptions
    {
        [JsonPropertyName(WorkspaceOptionDefinitions.WorkspaceIdOrName)]
        public string? Workspace { get; set; }
    }
}
