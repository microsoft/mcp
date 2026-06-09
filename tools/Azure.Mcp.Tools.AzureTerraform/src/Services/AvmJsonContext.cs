// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

[JsonSerializable(typeof(List<GitHubRelease>))]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
internal partial class AvmJsonContext : JsonSerializerContext;
