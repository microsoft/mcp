// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;

public sealed class DrillRunGetReportDownloadUrlOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.Drill)]
    public required string Drill { get; set; }

    [Option(Description = "The name of the drill run whose generated report will be downloaded.")]
    public required string DrillRun { get; set; }

    [Option(Description = "Optional report format to download. Allowed value: Html (the default).")]
    public string? Format { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
