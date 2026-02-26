// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;

public class BaseWebappDiagnosticOptions : BaseAppServiceOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.DiagnosticCategoryName)]
    public string? DiagnosticCategory { get; set; }
}
