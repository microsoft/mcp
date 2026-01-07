// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace ToolMetadataExporter.Models;

internal class CommandLineOptions
{
    public bool IsDryRun { get; set; } = false;

    public string? AzmcpExe { get; set; }
}
