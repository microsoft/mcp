// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Extension.Models;

public sealed class AzureCliGenerateRequest
{
    [JsonPropertyName("question")]
    public required string Question { get; set; }

    [JsonPropertyName("history")]
    public required AzureCliCopilotHistory[] History { get; set; }

    [JsonPropertyName("enable_parameter_injection")]
    public bool? EnableParameterInjection;
}

public sealed class AzureCliCopilotHistory
{
    public required string Content;
    public required string Role;
}
