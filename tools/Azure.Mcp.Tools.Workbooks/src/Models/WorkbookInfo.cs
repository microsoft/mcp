// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Models;

public sealed record WorkbookInfo(
    string WorkbookId,
    string? DisplayName,
    string? Description,
    string? Category,
    string? Location,
    string? Kind,
    string? Tags,
    string? SerializedData,
    string? Version,
    DateTimeOffset? TimeModified,
    string? UserId,
    string? SourceId
);
