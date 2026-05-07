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

    public const string DataflowIdName = "dataflow-id";
    public static readonly Option<string> DataflowId = new($"--{DataflowIdName}")
    {
        Description = "The ID of the dataflow.",
        Required = true
    };

    public const string QueryNameName = "query-name";
    public static readonly Option<string> QueryName = new($"--{QueryNameName}")
    {
        Description = "The name of the query to execute.",
        Required = true
    };

    public const string QueryTextName = "query";
    public static readonly Option<string> QueryText = new($"--{QueryTextName}")
    {
        Description = "The M (Power Query) expression to execute.",
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
