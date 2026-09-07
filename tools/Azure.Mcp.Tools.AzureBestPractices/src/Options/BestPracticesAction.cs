// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBestPractices.Options;

public enum BestPracticesAction
{
    [JsonStringEnumMemberName("all")]
    All,

    [JsonStringEnumMemberName("code-generation")]
    CodeGeneration,

    [JsonStringEnumMemberName("deployment")]
    Deployment
}
