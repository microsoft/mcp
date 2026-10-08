// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Log;

public sealed record LogSearchError(
    string Code,
    string Message,
    IReadOnlyList<LogSearchErrorDetail> Details);
