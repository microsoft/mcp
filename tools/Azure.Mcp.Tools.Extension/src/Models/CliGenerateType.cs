// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Extension.Models;

public enum CliGenerateType
{
    [JsonStringEnumMemberName("az")]
    Az
}
