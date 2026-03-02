// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;

public sealed class DetectorDiagnoseOptions : BaseAppServiceOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.DetectorNameName)]
    public string? DetectorName { get; set; }
}
