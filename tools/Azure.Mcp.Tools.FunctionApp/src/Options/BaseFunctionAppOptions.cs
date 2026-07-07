// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FunctionApp.Options;

public class BaseFunctionAppOptions : SubscriptionOptions
{
    [JsonPropertyName(FunctionAppOptionDefinitions.FunctionAppName)]
    public string? FunctionAppName { get; set; }
}
