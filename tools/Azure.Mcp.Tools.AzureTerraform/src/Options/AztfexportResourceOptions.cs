// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AztfexportResourceOptions
{
    public string? ResourceId { get; set; }
    public string? OutputFolderName { get; set; }
    public string? Provider { get; set; }
    public string? ResourceName { get; set; }
    public bool IncludeRoleAssignment { get; set; }
    public int Parallelism { get; set; } = 10;
    public bool ContinueOnError { get; set; } = true;
}
