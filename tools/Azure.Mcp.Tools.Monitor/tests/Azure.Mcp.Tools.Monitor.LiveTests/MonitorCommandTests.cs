// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.LiveTests;

public class MonitorCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    private LogAnalyticsHelper? _logHelper;
    private const string TestLogType = "TestLogs_CL";
    private IMonitorService? _monitorService;
    private string? _storageAccountName;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _monitorService = GetMonitorService();
        _storageAccountName = $"{Settings.ResourceBaseName}mon";
        _logHelper = new LogAnalyticsHelper(Settings.ResourceBaseName, Settings.SubscriptionId, _monitorService, Settings.TenantId, TestLogType);
    }

    private static IMonitorService GetMonitorService()
    {
        var memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        var cacheService = new CacheService(memoryCache);
        var tenantService = new TenantService(cacheService);
        var subscriptionService = new SubscriptionService(cacheService, tenantService);
        var resourceGroupService = new ResourceGroupService(cacheService, subscriptionService);
        return new MonitorService(subscriptionService, tenantService, resourceGroupService);
    }

    [Fact]
    public async Task Should_list_monitor_tables()
    {
        var result = await CallToolAsync(
            "azmcp_monitor_table_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "workspace", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "table-type", "Microsoft" }
            });

        var tablesArray = result.AssertProperty("tables");
        Assert.Equal(JsonValueKind.Array, tablesArray.ValueKind);
        var array = tablesArray.EnumerateArray();
        Assert.NotEmpty(array);
    }

    [Fact]
    public async Task Should_list_monitor_workspaces()
    {
        var result = await CallToolAsync(
            "azmcp_monitor_workspace_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var workspacesArray = result.AssertProperty("workspaces");
        Assert.Equal(JsonValueKind.Array, workspacesArray.ValueKind);
        var array = workspacesArray.EnumerateArray();
        Assert.NotEmpty(array);
    }

    [Fact]
    public async Task Should_get_table_contents()
    {
        // Query AzureMetrics table - fastest to propagate and most reliable
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("staticResourceGroup", "static-test-resources");
        var workspace = Settings.DeploymentOutputs.GetValueOrDefault("staticWorkspace", "monitor-query-ws");
        await QueryForLogsAsync(
            async args => await CallToolAsync("azmcp_monitor_workspace_log_query", args),
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "workspace", workspace },
                { "resource-group", resourceGroup },
                { "query", "AzureMetrics | where ResourceProvider == 'MICROSOFT.STORAGE' | project TimeGenerated, MetricName, Total, ResourceId" },
                { "table", "AzureMetrics" },
                { "limit", 5 },
                { "hours", 24 }
            },
            $"AzureMetrics | where ResourceProvider == 'MICROSOFT.STORAGE' | project TimeGenerated, MetricName, Total, ResourceId",
            sendLogInfo: null,
            sendLogAction: null,
            output: Output,
            cancellationToken: TestContext.Current.CancellationToken,
            maxWaitTimeSeconds: 180, // 3 minutes - metrics are faster than logs
            failMessage: "No storage metrics found after waiting 180 seconds");
    }

    [Fact]
    public async Task Should_query_monitor_logs()
    {
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("staticResourceGroup", "static-test-resources");
        var workspace = Settings.DeploymentOutputs.GetValueOrDefault("staticWorkspace", "monitor-query-ws");
        await QueryForLogsAsync(
            async args => await CallToolAsync("azmcp_monitor_workspace_log_query", args),
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "workspace", workspace },
                { "resource-group", resourceGroup },
                { "table", "StorageBlobLogs" },
                { "query", "StorageBlobLogs | project TimeGenerated, OperationName, StatusText" },
                { "limit", 1 },
                { "hours", 24 }
            },
            $"StorageBlobLogs | project TimeGenerated, OperationName, StatusText",
            sendLogInfo: null,
            sendLogAction: null,
            cancellationToken: TestContext.Current.CancellationToken,
            maxWaitTimeSeconds: 300, // 5 minutes - realistic for storage diagnostic logs
            failMessage: "No storage blob logs found after waiting 300 seconds");
    }

    [Fact]
    public async Task Should_list_monitor_table_types()
    {
        var result = await CallToolAsync(
            "azmcp_monitor_table_type_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "workspace", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName }
            });

        var tableTypesArray = result.AssertProperty("tableTypes");
        Assert.Equal(JsonValueKind.Array, tableTypesArray.ValueKind);
        var array = tableTypesArray.EnumerateArray();
        Assert.NotEmpty(array);
    }

    [Fact]
    public async Task Should_query_monitor_logs_by_resource_id()
    {
        var subscriptionId = Settings.SubscriptionId;
        var resourceGroup = Settings.DeploymentOutputs.GetValueOrDefault("staticResourceGroup", "static-test-resources");
        var storageAccountName = Settings.DeploymentOutputs.GetValueOrDefault("staticStorageAccountName", "azuresdktrainingdatatme");

        var storageResourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}";

        await QueryForLogsAsync(
            async args => await CallToolAsync("azmcp_monitor_resource_log_query", args),
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-id", storageResourceId },
                { "table", "StorageBlobLogs" },
                { "query", "StorageBlobLogs | project TimeGenerated, OperationName, StatusText" },
                { "limit", 1 },
                { "hours", 24 }
            },
            $"StorageBlobLogs | where TimeGenerated > datetime({DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}) | project TimeGenerated, OperationName, StatusText",
            sendLogInfo: null,
            sendLogAction: null,
            output: Output,
            cancellationToken: TestContext.Current.CancellationToken,
            maxWaitTimeSeconds: 300, // 5 minutes - realistic for storage diagnostic logs
            failMessage: "No storage blob logs found for resource after waiting 300 seconds");
    }

    private static async Task QueryForLogsAsync(
        Func<Dictionary<string, object?>, Task<JsonElement?>> callToolAsync,
        Dictionary<string, object?> initialQueryArgs,
        string logQuery,
        string? sendLogInfo = null,
        Func<Task>? sendLogAction = null,
        ITestOutputHelper? output = null,
        CancellationToken cancellationToken = default,
        int maxWaitTimeSeconds = 60,
        string? failMessage = null)
    {
        // First try to find any existing logs
        output?.WriteLine($"Checking for existing logs...");
        var queryStartTime = DateTime.UtcNow;
        var result = await callToolAsync(initialQueryArgs);
        Assert.NotNull(result);
        Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
        var logs = result.Value.EnumerateArray();
        var queryDuration = (DateTime.UtcNow - queryStartTime).TotalSeconds;

        if (logs.Any())
        {
            output?.WriteLine($"Found existing logs");
            output?.WriteLine($"Query performance: {queryDuration:F1}s to execute");
            return;
        }

        if (sendLogAction != null)
        {
            output?.WriteLine($"No recent logs found, sending new log...");
            await sendLogAction();
            output?.WriteLine(sendLogInfo ?? "Info log sent.");
        }

        // Start time for query window - use the current time
        var testStartTime = DateTime.UtcNow;
        output?.WriteLine($"Starting to query for new log (max wait: {maxWaitTimeSeconds}s)...");
        var attemptCount = 0;

        while ((DateTime.UtcNow - testStartTime).TotalSeconds < maxWaitTimeSeconds)
        {
            // More aggressive polling at start (1s, 2s, 4s, 8s, 15s...)
            var delaySeconds = Math.Min(Math.Pow(2, attemptCount), 15);
            attemptCount++;

            var elapsed = (DateTime.UtcNow - testStartTime).TotalSeconds;
            output?.WriteLine($"Attempt {attemptCount}: Querying for logs at {elapsed:F1}s...");

            queryStartTime = DateTime.UtcNow;
            var queryArgs = new Dictionary<string, object?>(initialQueryArgs)
            {
                ["query"] = logQuery
            };
            result = await callToolAsync(queryArgs);
            queryDuration = (DateTime.UtcNow - queryStartTime).TotalSeconds;
            output?.WriteLine($"Query completed in {queryDuration:F1} seconds");

            Assert.NotNull(result);
            Assert.Equal(JsonValueKind.Array, result.Value.ValueKind);
            logs = result.Value.EnumerateArray();
            if (logs.Any())
            {
                var totalTime = (DateTime.UtcNow - testStartTime).TotalSeconds;
                output?.WriteLine($"Success! Found new log after {totalTime:F1} seconds (attempt {attemptCount})");
                output?.WriteLine($"Query performance: {queryDuration:F1}s to execute, {totalTime:F1}s total test time");
                return;
            }

            output?.WriteLine($"No logs found yet (attempt {attemptCount}), waiting {delaySeconds:F1} seconds before retrying...");
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
        }

        Assert.Fail(failMessage ?? $"No logs found after waiting {maxWaitTimeSeconds} seconds");
    }

    [Fact]
    public async Task Should_list_metric_definitions()
    {
        // Example resource ID - uses a storage account that should exist from the test fixture
        string resourceId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{_storageAccountName}";

        var result = await CallToolAsync(
            "azmcp_monitor_metrics_definitions",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource", _storageAccountName },
                { "resource-type", "Microsoft.Storage/storageAccounts" }
            });

        var resultsArray = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, resultsArray.ValueKind);
        Assert.NotEmpty(resultsArray.EnumerateArray());

        // Validate the status message
        var status = result.AssertProperty("status");
        Assert.Equal(JsonValueKind.String, status.ValueKind);
        var statusString = status.GetString();
        Assert.NotNull(statusString);
        Assert.Contains("metric definitions returned", statusString);
        Assert.StartsWith("All", statusString);

        // Validate at least one metric definition has all expected properties populated
        var firstDefinition = resultsArray.EnumerateArray().First();

        // Verify required properties exist and are populated
        var name = firstDefinition.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, name.ValueKind);
        Assert.False(string.IsNullOrEmpty(name.GetString()));

        var category = firstDefinition.AssertProperty("category");
        Assert.Equal(JsonValueKind.String, category.ValueKind);
        Assert.False(string.IsNullOrEmpty(category.GetString()));

        var description = firstDefinition.AssertProperty("description");
        Assert.Equal(JsonValueKind.String, description.ValueKind);
        Assert.False(string.IsNullOrEmpty(description.GetString()));

        var unit = firstDefinition.AssertProperty("unit");
        Assert.Equal(JsonValueKind.String, unit.ValueKind);
        Assert.False(string.IsNullOrEmpty(unit.GetString()));

        var defaultAggregation = firstDefinition.AssertProperty("defaultAggregation");
        Assert.Equal(JsonValueKind.String, defaultAggregation.ValueKind);
        Assert.False(string.IsNullOrEmpty(defaultAggregation.GetString()));

        var supportedAggregationTypes = firstDefinition.AssertProperty("supportedAggregationTypes");
        Assert.Equal(JsonValueKind.Array, supportedAggregationTypes.ValueKind);
        Assert.NotEmpty(supportedAggregationTypes.EnumerateArray());

        var isDimensionRequired = firstDefinition.AssertProperty("isDimensionRequiredWhenQuerying");
        Assert.Equal(JsonValueKind.False, isDimensionRequired.ValueKind);

        var metricNamespace = firstDefinition.AssertProperty("metricNamespace");
        Assert.Equal(JsonValueKind.String, metricNamespace.ValueKind);
        Assert.False(string.IsNullOrEmpty(metricNamespace.GetString()));

        var allowedIntervals = firstDefinition.AssertProperty("allowedIntervals");
        Assert.Equal(JsonValueKind.Array, allowedIntervals.ValueKind);
        Assert.NotEmpty(allowedIntervals.EnumerateArray());

        var dimensions = firstDefinition.AssertProperty("dimensions");
        Assert.Equal(JsonValueKind.Array, dimensions.ValueKind);
        // Dimensions array can be empty, so we just verify it exists and is an array
    }

    [Fact]
    public async Task Should_query_metrics()
    {
        // Example resource ID - uses a storage account that should exist from the test fixture
        string resourceId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Storage/storageAccounts/{_storageAccountName}";

        var result = await CallToolAsync(
            "azmcp_monitor_metrics_query",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource", _storageAccountName },
                { "resource-type", "Microsoft.Storage/storageAccounts" },
                { "metric-namespace", "Microsoft.storage/storageAccounts" },
                { "metric-names", "UsedCapacity" } // Common storage account metric
            });

        var resultsArray = result.AssertProperty("results");
        Assert.Equal(JsonValueKind.Array, resultsArray.ValueKind);
        Assert.NotEmpty(resultsArray.EnumerateArray());

        // Validate the first metric has all expected properties
        var firstMetric = resultsArray.EnumerateArray().First();

        // Verify metric-level properties
        var name = firstMetric.AssertProperty("name");
        Assert.Equal(JsonValueKind.String, name.ValueKind);
        Assert.False(string.IsNullOrEmpty(name.GetString()));

        var unit = firstMetric.AssertProperty("unit");
        Assert.Equal(JsonValueKind.String, unit.ValueKind);
        Assert.False(string.IsNullOrEmpty(unit.GetString()));

        var timeSeries = firstMetric.AssertProperty("timeSeries");
        Assert.Equal(JsonValueKind.Array, timeSeries.ValueKind);
        Assert.NotEmpty(timeSeries.EnumerateArray());

        // Validate the first timeSeries entry has all expected properties
        var firstTimeSeries = timeSeries.EnumerateArray().First();

        var metadata = firstTimeSeries.AssertProperty("metadata");
        Assert.Equal(JsonValueKind.Object, metadata.ValueKind);

        var start = firstTimeSeries.AssertProperty("start");
        Assert.Equal(JsonValueKind.String, start.ValueKind);
        Assert.False(string.IsNullOrEmpty(start.GetString()));
        // Verify it's a valid ISO date format
        Assert.True(DateTime.TryParse(start.GetString(), out _));

        var end = firstTimeSeries.AssertProperty("end");
        Assert.Equal(JsonValueKind.String, end.ValueKind);
        Assert.False(string.IsNullOrEmpty(end.GetString()));
        // Verify it's a valid ISO date format
        Assert.True(DateTime.TryParse(end.GetString(), out _));

        var interval = firstTimeSeries.AssertProperty("interval");
        Assert.Equal(JsonValueKind.String, interval.ValueKind);
        Assert.False(string.IsNullOrEmpty(interval.GetString()));
        // Verify it follows duration format (starts with PT)
        Assert.StartsWith("PT", interval.GetString());
    }

    private async Task GenerateStorageActivityAsync()
    {
        try
        {
            // First, generate basic activity (creates metrics)
            var listResult = await CallToolAsync("azmcp_storage_blob_container_list", new()
            {
                { "subscription", Settings.SubscriptionId },
                { "account", _storageAccountName }
            });

            Output.WriteLine("Listed storage containers to generate metrics");

            // Try to list blobs in a container if any exist (also generates metrics)
            var containersArray = listResult?.GetProperty("containers");
            if (containersArray?.ValueKind == JsonValueKind.Array && containersArray.Value.EnumerateArray().Any())
            {
                var firstContainer = containersArray.Value.EnumerateArray().First();
                if (firstContainer.TryGetProperty("name", out var containerName))
                {
                    var blobListResult = await CallToolAsync("azmcp_storage_blob_list", new()
                    {
                        { "subscription", Settings.SubscriptionId },
                        { "account", _storageAccountName },
                        { "container", containerName.GetString() }
                    });

                    Output.WriteLine($"Listed blobs in container '{containerName.GetString()}' to generate metrics");

                    // Try to get properties of a blob if any exist (generates StorageBlobLogs)
                    var blobsArray = blobListResult?.GetProperty("blobs");
                    if (blobsArray?.ValueKind == JsonValueKind.Array && blobsArray.Value.EnumerateArray().Any())
                    {
                        var firstBlob = blobsArray.Value.EnumerateArray().First();
                        if (firstBlob.TryGetProperty("name", out var blobName))
                        {
                            try
                            {
                                // Note: This would require a blob details command if available
                                // For now, the list operations should generate some transaction logs
                                Output.WriteLine($"Found blob '{blobName.GetString()}' - operations should generate diagnostic logs");
                            }
                            catch
                            {
                                // Ignore blob property errors
                            }
                        }
                    }
                }
            }

            // Even if no blobs exist, the container/blob listing operations
            // will generate transaction metrics that should appear in AzureMetrics table
            Output.WriteLine("Storage operations completed - should generate metrics and potentially some blob logs");
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Note: Storage activity generation encountered an issue: {ex.Message}");
            // Don't fail the test if storage activity generation fails
        }
    }
}

