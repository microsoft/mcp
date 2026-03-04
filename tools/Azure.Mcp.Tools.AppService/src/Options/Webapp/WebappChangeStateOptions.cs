// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp;

public class WebappChangeStateOptions : BaseAppServiceOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.StateChangeName)]
    public string? StateChange { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.SoftRestartName)]
    public bool? SoftRestart { get; set; }

    [JsonPropertyName(AppServiceOptionDefinitions.WaitForCompletionName)]
    public bool? WaitForCompletion { get; set; }
}
