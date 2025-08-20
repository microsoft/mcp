// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Workbooks.Options.Workbook;

public class DeleteWorkbookOptions : GlobalOptions
{
    [JsonPropertyName(WorkbooksOptionDefinitions.WorkbookIdText)]
    public string? WorkbookId { get; set; }
}
