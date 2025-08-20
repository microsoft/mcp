// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options;

public static class WorkspaceOptionDefinitions
{
    public const string WorkspaceIdOrName = "workspace";

    public static readonly Option<string> Workspace = new(
        $"--{WorkspaceIdOrName}",
        "The Log Analytics workspace ID or name. This can be either the unique identifier (GUID) or the display name of your workspace."
    )
    {
        IsRequired = true
    };

}
