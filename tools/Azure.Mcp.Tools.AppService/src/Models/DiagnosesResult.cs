// Copyright (c) Microsoft Corporation.Expand commentComment on line R1Resolved
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.AppService.Models;

namespace Azure.Mcp.Tools.AppService.Models;

/// <summary>
/// Represents diagnoses results after running a Web App detector.
/// </summary>
public sealed record DiagnosesResults(
    [property: JsonPropertyName("dataset")] IList<DiagnosticDataset> Dataset,
    [property: JsonPropertyName("metadata")] DetectorInfo Metadata);