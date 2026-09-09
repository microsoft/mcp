// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Extension.Models;

public enum CliInstallType
{
    [JsonStringEnumMemberName("az")]
    Az,

    [JsonStringEnumMemberName("azd")]
    Azd,

    [JsonStringEnumMemberName("func")]
    Func
}
