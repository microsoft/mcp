// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Extension.Commands;

// This file should only contain a single definition of AzqrReportResult
public sealed record AzqrReportResult(string XlsxReportPath, string JsonReportPath, string Stdout);
