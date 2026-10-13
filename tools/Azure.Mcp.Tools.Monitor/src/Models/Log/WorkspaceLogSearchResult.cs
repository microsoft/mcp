// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Monitor.Models.Log;

public sealed record WorkspaceLogSearchResult(
    string Table,
    string Plan,
    string Timespan,
    IReadOnlyList<LogSearchColumn> Columns,
    IReadOnlyList<IReadOnlyList<JsonElement>> Rows,
    int RowCount,
    int Limit,
    bool IsPartial,
    LogSearchError? Error);
