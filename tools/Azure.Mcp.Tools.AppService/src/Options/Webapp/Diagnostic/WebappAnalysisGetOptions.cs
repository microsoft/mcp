// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;

public class WebappAnalysisGetOptions : BaseWebappDiagnosticOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.AnalysisNameName)]
    public string? AnalysisName { get; set; }
}
