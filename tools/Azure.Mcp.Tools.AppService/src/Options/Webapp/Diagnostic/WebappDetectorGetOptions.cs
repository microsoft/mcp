// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;

public class WebappDetectorGetOptions : BaseWebappDiagnosticOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.DetectorNameName)]
    public string? DetectorName { get; set; }
}
