// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBestPractices.Options;

public enum BestPracticesResource
{
    [JsonStringEnumMemberName("general")]
    General,

    [JsonStringEnumMemberName("azurefunctions")]
    AzureFunctions,

    [JsonStringEnumMemberName("static-web-app")]
    StaticWebApp,

    [JsonStringEnumMemberName("coding-agent")]
    CodingAgent
}
