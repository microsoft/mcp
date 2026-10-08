// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Log;

internal readonly record struct LogSearchTimeRange(
    DateTimeOffset Start,
    DateTimeOffset End)
{
    public TimeSpan Duration => End - Start;
}
