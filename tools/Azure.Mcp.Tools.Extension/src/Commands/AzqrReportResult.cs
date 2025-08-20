// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Extension.Commands;

// This file should only contain a single definition of AzqrReportResult
public sealed record AzqrReportResult(
    [property: JsonPropertyName("xlsxReportPath")] string XlsxReportPath,
    [property: JsonPropertyName("jsonReportPath")] string JsonReportPath,
    [property: JsonPropertyName("stdout")] string Stdout
);
