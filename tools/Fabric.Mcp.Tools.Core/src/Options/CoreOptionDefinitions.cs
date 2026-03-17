// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Fabric.Mcp.Tools.Core.Options;

public static class CoreOptionDefinitions
{
    // Workspace options
    public const string WorkspaceName = "workspace";
    public static readonly Option<string> Workspace = new($"--{WorkspaceName}")
    {
        Description = "The name or ID of the Microsoft Fabric workspace.",
        Required = true
    };

    public const string WorkspaceIdName = "workspace-id";
    public static readonly Option<string> WorkspaceId = new($"--{WorkspaceIdName}")
    {
        Description = "The ID of the Microsoft Fabric workspace.",
        Required = true
    };

    // Item options
    public const string ItemTypeName = "item-type";
    public static readonly Option<string> ItemType = new($"--{ItemTypeName}")
    {
        Description = "The type of the Fabric item (e.g., Lakehouse, Notebook, etc.).",
        Required = false
    };

    // Display options
    public const string DisplayNameName = "display-name";
    public static readonly Option<string> DisplayName = new($"--{DisplayNameName}")
    {
        Description = "The display name for the item.",
        Required = true
    };

    public const string DescriptionName = "description";
    public static readonly Option<string> Description = new($"--{DescriptionName}")
    {
        Description = "The description for the item.",
        Required = false
    };
}
