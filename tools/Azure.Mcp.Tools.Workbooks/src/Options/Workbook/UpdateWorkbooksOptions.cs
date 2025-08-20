// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Workbooks.Options.Workbook;

public class UpdateWorkbooksOptions : BaseWorkbooksOptions
{
    [JsonPropertyName(WorkbooksOptionDefinitions.WorkbookIdText)]
    public string? WorkbookId { get; set; }

    [JsonPropertyName(WorkbooksOptionDefinitions.DisplayNameText)]
    public string? DisplayName { get; set; }

    [JsonPropertyName(WorkbooksOptionDefinitions.SerializedContentText)]
    public string? SerializedContent { get; set; }
}
