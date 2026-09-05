// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Log;

internal sealed class LogSearchApiResponse
{
    public List<LogSearchApiTable>? Tables { get; set; }

    public LogSearchApiError? Error { get; set; }
}
