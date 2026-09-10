// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Monitor.Models.Log;

internal sealed class LogSearchApiTable
{
    public string? Name { get; set; }

    public List<LogSearchApiColumn>? Columns { get; set; }

    public List<List<JsonElement>>? Rows { get; set; }
}
