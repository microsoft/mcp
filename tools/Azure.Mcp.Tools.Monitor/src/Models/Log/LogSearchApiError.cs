// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Log;

internal sealed class LogSearchApiError
{
    public string? Code { get; set; }

    public string? Message { get; set; }

    public List<LogSearchApiErrorDetail>? Details { get; set; }
}
