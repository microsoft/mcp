// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ApplicationInsights.Models;

public class AppLogsQueryRow<T>
{
    public required T Data { get; set; }
    public Dictionary<string, object?> OtherColumns { get; set; } = new Dictionary<string, object?>();
}
