// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Extension.Models;

public sealed record CliInstallResult(
    [property: JsonPropertyName("installationInstructions")] string InstallationInstructions,
    [property: JsonPropertyName("cliType")] string CliType
);
