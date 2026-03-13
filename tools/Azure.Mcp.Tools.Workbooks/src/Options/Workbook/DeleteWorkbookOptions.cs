// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Workbooks.Options.Workbook;

public class DeleteWorkbookOptions : BaseWorkbooksOptions
{
    [JsonPropertyName(WorkbooksOptionDefinitions.WorkbookIdsText)]
    public string[]? WorkbookIds { get; set; }
}
