// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Extension.Models;

public sealed record AzureCliGenerateRequest(
    string Question,
    AzureCliCopilotHistory[] History,
    [property: JsonPropertyName("enable_parameter_injection")] bool? EnableParameterInjection);

public sealed record AzureCliCopilotHistory(string Content, string Role);
