// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Fabric.Mcp.Tools.DataFactory.Options;

public static class DataFactoryOptionDefinitions
{
    public const string WorkspaceIdName = "workspace-id";
    public static readonly Option<string> WorkspaceId = new($"--{WorkspaceIdName}")
    {
        Description = "The ID of the Microsoft Fabric workspace.",
        Required = true
    };

    public const string PipelineIdName = "pipeline-id";
    public static readonly Option<string> PipelineId = new($"--{PipelineIdName}")
    {
        Description = "The ID of the pipeline.",
        Required = true
    };

    public const string DisplayNameName = "display-name";
    public static readonly Option<string> DisplayName = new($"--{DisplayNameName}")
    {
        Description = "The display name for the item.",
        Required = true
    };

    public const string DescriptionName = "description";
    public static readonly Option<string> Description = new($"--{DescriptionName}")
    {
        Description = "Optional description for the item.",
        Required = false
    };

    public const string RolesName = "roles";
    public static readonly Option<string> Roles = new($"--{RolesName}")
    {
        Description = "Filter workspaces by roles (Admin, Member, Contributor, Viewer).",
        Required = false
    };
}
