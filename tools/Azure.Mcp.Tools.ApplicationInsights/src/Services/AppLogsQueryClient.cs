// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Azure.Core;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.Monitor.Query;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public class AppLogsQueryClient(LogsQueryClient logsQueryClient) : IAppLogsQueryClient
{
    private readonly LogsQueryClient _logsQueryClient = logsQueryClient;

    public async Task<IReadOnlyList<AppLogsQueryRow<T>>> QueryResourceAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(ResourceIdentifier resourceId, string kql, QueryTimeRange timeRange) where T : new()
    {
        var response = await _logsQueryClient.QueryResourceAsync(
            resourceId,
            kql,
            timeRange);

        PropertyInfo[] destinationProperties = typeof(T).GetProperties();
        // Convert data into T
        // First we need to know the column indices relevant for conversion and the destination property indices they convert to
        Dictionary<int, int> conversionMap = new Dictionary<int, int>();
        var columns = response.Value.Table.Columns;
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            for (int j = 0; j < destinationProperties.Length; j++)
            {
                PropertyInfo info = destinationProperties[j];
                if (column.Name.Equals(info.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    conversionMap[i] = j;
                    break;
                }
            }
        }

        // Now we can generate the final data



        var rows = response.Value.Table.Rows.Select(row =>
        {
            Dictionary<string, object?> otherColumns = new Dictionary<string, object?>();

            T retObj = new T();
            for (int i = 0; i < row.Count; i++)
            {
                if (conversionMap.TryGetValue(i, out int j))
                {
                    PropertyInfo property = destinationProperties[j];
                    Type destType = property.PropertyType;
                    var currentToken = row[i];

                    if (destType == typeof(string))
                    {
                        // force conversion to string
                        property.SetValue(retObj, currentToken.ToString());
                    }
                    else
                    {
                        property.SetValue(retObj, currentToken);
                    }
                }
                else
                {
                    otherColumns[columns[i].Name] = row[i];
                }
            }

            return new AppLogsQueryRow<T>
            {
                Data = retObj,
                OtherColumns = otherColumns
            };
        }).ToList(); // Use ToList to force conversion to happen now, not on demand

        return rows;
    }
}
