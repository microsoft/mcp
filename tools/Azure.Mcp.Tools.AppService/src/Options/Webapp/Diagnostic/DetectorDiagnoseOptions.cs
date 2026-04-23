// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;

public sealed class DetectorDiagnoseOptions : BaseAppServiceOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.DetectorIdName)]
    public string? DetectorId { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.StartTimeName)]
    public DateTimeOffset? StartTime { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.EndTimeName)]
    public DateTimeOffset? EndTime { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.IntervalName)]
    public string? Interval { get; set; }
}
