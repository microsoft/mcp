// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.Text.Json;
using Azure.Mcp.Tools.ApplicationInsights.Commands;

namespace Azure.Mcp.Tools.ApplicationInsights.Models.Query;

public static class QueryToResponseModelConversions
{
    public static AppListTraceEntry ToResponseModel(this AppLogsQueryRow<ListTraceQueryResponse> row)
    {
        return new AppListTraceEntry
        {
            ProblemId = row.Data.problemId,
            Target = row.Data.target,
            TestLocation = row.Data.location,
            TestName = row.Data.name,
            Type = row.Data.type,
            OperationName = row.Data.operation_Name,
            ResultCode = row.Data.resultCode,
            Traces = string.IsNullOrEmpty(row.Data.traces) ? new List<TraceIdEntry>() : (JsonSerializer.Deserialize(row.Data.traces!, ApplicationInsightsJsonContext.Default.ListTraceIdEntry) ?? []).Distinct().ToList()
        };
    }
}
