// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Extension.Models;

public sealed record CliGenerateResult(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("cliType")] string CliType
);
