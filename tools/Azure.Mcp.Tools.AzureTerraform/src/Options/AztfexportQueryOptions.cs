// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AztfexportQueryOptions
{
    public string? Query { get; set; }
    public string? OutputFolderName { get; set; }
    public string? Provider { get; set; }
    public string? NamePattern { get; set; }
    public bool IncludeRoleAssignment { get; set; }
    public int Parallelism { get; set; } = 10;
    public bool ContinueOnError { get; set; } = true;
}
