// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Xml;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Routing;
using Azure.ResourceManager;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.IoTHub.Services;

public class IoTHubRoutingService(
    IAzureService azureService,
    IHttpClientFactory httpClientFactory,
    ILogger<IoTHubRoutingService> logger)
    : BaseAzureService(azureService), IIoTHubRoutingService
{
    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<IoTHubRoutingService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private static readonly TimeSpan s_operationTimeout = TimeSpan.FromSeconds(100);

    internal static async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken,
        TimeSpan? timeout = null)
    {
        var operationTimeout = timeout ?? s_operationTimeout;
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(operationTimeout);
        try
        {
            timeoutCts.Token.ThrowIfCancellationRequested();
            return await operation(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"The IoT Hub '{operationName}' operation timed out after {operationTimeout.TotalSeconds:N0} seconds.");
        }
    }

    public async Task<List<RoutingEndpointHealthSnapshot>> GetRoutingEndpointHealth(
        string hubName,
        string resourceGroup,
        string subscription,
        string? endpointName = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateInputs(hubName, resourceGroup, subscription);

        try
        {
            return await ExecuteWithTimeoutAsync<List<RoutingEndpointHealthSnapshot>>(async ct =>
            {
                var hub = await GetIoTHubGenericResourceAsync(
                    hubName,
                    resourceGroup,
                    subscription,
                    tenant,
                    ct);
                var properties = hub.Properties?.ToObjectFromJson(IoTHubJsonContext.Default.IoTHubProperties);
                var endpoints = FilterEndpoints(
                    RoutingEndpointMapper.ConvertToRoutingEndpointDetailsList(properties?.Routing?.Endpoints),
                    endpointName,
                    hubName);
                var endpointNames = endpoints
                    .Where(endpoint => !string.IsNullOrWhiteSpace(endpoint.EndpointId))
                    .ToDictionary(
                        endpoint => endpoint.EndpointId!,
                        endpoint => endpoint.Name,
                        StringComparer.OrdinalIgnoreCase);
                var healthRecords = await GetRoutingEndpointsHealthAsync(
                    hub.Id,
                    tenant,
                    ct);

                return
                [
                    .. healthRecords
                        .Where(record =>
                            !string.IsNullOrWhiteSpace(record.EndpointId) &&
                            endpointNames.ContainsKey(record.EndpointId))
                        .Select(record => new RoutingEndpointHealthSnapshot(
                            EndpointId: record.EndpointId!,
                            EndpointName: endpointNames[record.EndpointId!],
                            HealthStatus: record.HealthStatus,
                            LastKnownError: record.LastKnownError,
                            LastKnownErrorTime: record.LastKnownErrorTime,
                            LastSuccessfulSendAttemptTime: record.LastSuccessfulSendAttemptTime,
                            LastSendAttemptTime: record.LastSendAttemptTime))
                ];
            }, "routing endpoint health", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving routing endpoint health for IoT Hub '{HubName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
                hubName,
                resourceGroup,
                subscription);
            throw;
        }
    }

    public async Task<RoutingEndpointDiagnostics> GetRoutingEndpointDiagnostics(
        string hubName,
        string resourceGroup,
        string subscription,
        string? endpointName = null,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        string? interval = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateInputs(hubName, resourceGroup, subscription);
        var resolvedWindow = RoutingDiagnosticsWindow.Resolve(startTime, endTime, interval);
        var window = new ObservationWindow(resolvedWindow.StartTime, resolvedWindow.EndTime);

        try
        {
            return await ExecuteWithTimeoutAsync(async ct =>
            {
                var hub = await GetIoTHubGenericResourceAsync(
                    hubName,
                    resourceGroup,
                    subscription,
                    tenant,
                    ct);
                var properties = hub.Properties?.ToObjectFromJson(IoTHubJsonContext.Default.IoTHubProperties);
                var endpoints = FilterEndpoints(
                    RoutingEndpointMapper.ConvertToRoutingEndpointDetailsList(properties?.Routing?.Endpoints),
                    endpointName,
                    hubName);
                var hubEvidence = await QueryHubEvidenceAsync(
                    hub.Id,
                    endpoints,
                    window,
                    resolvedWindow.Interval,
                    tenant,
                    ct);
                var targetEvidence = new ConcurrentDictionary<string, Lazy<Task<RoutingTargetEmitted>>>(
                    StringComparer.OrdinalIgnoreCase);

                var endpointResults = await Task.WhenAll(endpoints.Select(endpoint =>
                    BuildEndpointDiagnosticsAsync(
                        endpoint,
                        hubEvidence.GetValueOrDefault(endpoint.Name) ?? CreateEmptyHubEvidence(),
                        window,
                        resolvedWindow.Interval,
                        tenant,
                        targetEvidence,
                        ct)));

                return new RoutingEndpointDiagnostics(
                    new RoutingObservationWindow(
                        ToIsoString(window.StartTime),
                        ToIsoString(window.EndTime),
                        XmlConvert.ToString(resolvedWindow.Interval)),
                    [.. endpointResults]);
            }, "routing endpoint diagnostics", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving routing endpoint diagnostics for IoT Hub '{HubName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
                hubName,
                resourceGroup,
                subscription);
            throw;
        }
    }

    private async Task<RoutingEndpointDiagnostic> BuildEndpointDiagnosticsAsync(
        RoutingEndpointDetails endpoint,
        RoutingHubEmitted hubEvidence,
        ObservationWindow window,
        TimeSpan interval,
        string? tenant,
        ConcurrentDictionary<string, Lazy<Task<RoutingTargetEmitted>>> targetEvidence,
        CancellationToken cancellationToken)
    {
        var resolution = ResolveTarget(endpoint);
        if (resolution is null)
        {
            var resolutionStatus = GetTargetDescriptor(endpoint.EndpointType) is null
                ? "unsupported"
                : "unresolved";
            return CreateEndpointDiagnostic(
                endpoint,
                new RoutingTargetInfo(resolutionStatus, ExistenceStatus: "notChecked"),
                hubEvidence,
                CreateEmptyTargetEvidence(resolutionStatus));
        }

        var existenceTask = GetTargetExistenceAsync(
            resolution.RoutedResourceId,
            resolution.Descriptor.ApiVersion,
            endpoint.Name,
            tenant,
            cancellationToken);
        var evidenceTask = targetEvidence.GetOrAdd(
            resolution.ParentResourceId.ToString(),
            _ => new Lazy<Task<RoutingTargetEmitted>>(
                () => QueryTargetEvidenceAsync(
                    resolution.ParentResourceId,
                    resolution.Descriptor,
                    window,
                    interval,
                    tenant,
                    cancellationToken))).Value;

        await Task.WhenAll(existenceTask, evidenceTask);
        var existence = await existenceTask;
        var targetMetrics = await evidenceTask;
        var target = new RoutingTargetInfo(
            "resolved",
            resolution.ParentResourceId.ToString(),
            resolution.Descriptor.ResourceType,
            resolution.RoutedResourceId.ToString(),
            existence.Status,
            ToIsoString(existence.ObservedAt),
            existence.Error);
        return CreateEndpointDiagnostic(endpoint, target, hubEvidence, targetMetrics);
    }

    private static RoutingEndpointDiagnostic CreateEndpointDiagnostic(
        RoutingEndpointDetails endpoint,
        RoutingTargetInfo target,
        RoutingHubEmitted hubEvidence,
        RoutingTargetEmitted targetEvidence) => new(
            EndpointId: endpoint.EndpointId ?? string.Empty,
            Name: endpoint.Name,
            EndpointType: endpoint.EndpointType,
            AuthenticationType: endpoint.AuthenticationType,
            EndpointUri: endpoint.EndpointUri,
            SubscriptionId: endpoint.SubscriptionId,
            ResourceGroup: endpoint.ResourceGroup,
            EndpointResourceName: endpoint.EndpointResourceName,
            EntityPath: endpoint.EntityPath,
            ContainerName: endpoint.ContainerName,
            DatabaseName: endpoint.DatabaseName,
            BatchFrequencyInSeconds: endpoint.BatchFrequencyInSeconds,
            Target: target,
            HubEmitted: hubEvidence,
            TargetEmitted: targetEvidence);

    private async Task<Dictionary<string, RoutingHubEmitted>> QueryHubEvidenceAsync(
        ResourceIdentifier hubResourceId,
        IReadOnlyList<RoutingEndpointDetails> endpoints,
        ObservationWindow window,
        TimeSpan interval,
        string? tenant,
        CancellationToken cancellationToken)
    {
        const string metricNamespace = "Microsoft.Devices/IotHubs";
        var builders = endpoints.ToDictionary(
            endpoint => endpoint.Name,
            _ => new RoutingMetricEvidenceBuilder(metricNamespace),
            StringComparer.OrdinalIgnoreCase);
        var errors = endpoints.ToDictionary(
            endpoint => endpoint.Name,
            _ => new List<RoutingDiagnosticError>(),
            StringComparer.OrdinalIgnoreCase);
        if (endpoints.Count == 0)
        {
            return [];
        }

        var armClient = await CreateRoutingArmClientAsync(tenant, cancellationToken);
        var endpointFilter = BuildEndpointFilter(endpoints);
        foreach (var metricName in new[] { "RoutingDeliveries", "RoutingDeliveryLatency" })
        {
            var isDeliveryCount = metricName == "RoutingDeliveries";
            try
            {
                var options = BuildMonitorMetricsOptions(
                    metricName,
                    metricNamespace,
                    window,
                    interval,
                    isDeliveryCount ? "Total" : "Average",
                    isDeliveryCount
                        ? $"{endpointFilter} and Result eq '*' and FailureReasonCategory eq '*'"
                        : endpointFilter);
                var metrics = armClient.GetMonitorMetricsAsync(hubResourceId, options, cancellationToken);
                var metricReturned = false;
                await foreach (var metric in metrics.WithCancellation(cancellationToken))
                {
                    if (!string.Equals(metric.Name?.Value, metricName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    metricReturned = true;
                    var metricError = GetMetricResponseError(
                        metric, metricName, hubResourceId, "hubAzureMonitor", "Reader");
                    if (metricError is not null)
                    {
                        AddSharedError(errors, metricError);
                        foreach (var builder in builders.Values)
                        {
                            builder.MarkFailed(metricName);
                        }
                        continue;
                    }

                    foreach (var series in metric.Timeseries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var endpointName = GetDimensionValue(series, "EndpointName");
                        if (string.IsNullOrWhiteSpace(endpointName) ||
                            !builders.TryGetValue(endpointName, out var builder))
                        {
                            continue;
                        }

                        try
                        {
                            if (isDeliveryCount)
                            {
                                var dimensions = new Dictionary<string, string>(StringComparer.Ordinal);
                                AddDimensionIfPresent(series, dimensions, "Result");
                                AddDimensionIfPresent(series, dimensions, "FailureReasonCategory");
                                builder.AddCountSeries(
                                    metricName,
                                    "Total",
                                    dimensions,
                                    series.Data.Select(point => (point.TimeStamp, point.Total ?? point.Count)));
                            }
                            else
                            {
                                builder.AddAverageSeries(
                                    metricName,
                                    "Milliseconds",
                                    EmptyDimensions,
                                    series.Data.Select(point => (point.TimeStamp, point.Average)));
                            }
                        }
                        catch (Exception ex) when (ex is InvalidDataException or OverflowException)
                        {
                            builder.MarkFailed(metricName);
                            errors[endpointName].Add(CreateDiagnosticError(
                                ex, "hubAzureMonitor", metricName, hubResourceId.ToString(), "Reader",
                                "IoT Hub routing metrics could not be represented."));
                        }
                    }

                    if (metric.Timeseries.Count >= MetricSeriesLimit)
                    {
                        AddSharedError(errors, CreateSeriesLimitError(
                            metricName, hubResourceId, "hubAzureMonitor"));
                        foreach (var builder in builders.Values)
                        {
                            builder.MarkPartial(metricName);
                        }
                    }
                }

                if (!metricReturned)
                {
                    AddSharedError(errors, CreateMissingMetricError(metricName, hubResourceId, "hubAzureMonitor"));
                }
                foreach (var builder in builders.Values)
                {
                    if (metricReturned)
                    {
                        builder.MarkSuccessful(metricName);
                    }
                    else
                    {
                        builder.MarkFailed(metricName);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Unable to query '{MetricName}' for IoT Hub '{HubResourceId}'.",
                    metricName, hubResourceId);
                foreach (var builder in builders.Values)
                {
                    builder.MarkFailed(metricName);
                }
                AddSharedError(errors, CreateDiagnosticError(
                    ex, "hubAzureMonitor", metricName, hubResourceId.ToString(), "Reader",
                    "IoT Hub routing metrics could not be read."));
            }
        }

        return endpoints.ToDictionary(
            endpoint => endpoint.Name,
            endpoint => new RoutingHubEmitted(
                builders[endpoint.Name].Build(),
                errors[endpoint.Name]),
            StringComparer.OrdinalIgnoreCase);
    }

    private async Task<RoutingTargetEmitted> QueryTargetEvidenceAsync(
        ResourceIdentifier resourceId,
        RoutingTargetDescriptor descriptor,
        ObservationWindow window,
        TimeSpan interval,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var builder = new RoutingMetricEvidenceBuilder(descriptor.ResourceType);
        var errors = new List<RoutingDiagnosticError>();
        var armClient = await CreateRoutingArmClientAsync(tenant, cancellationToken);

        switch (descriptor.EndpointType)
        {
            case "EventHub":
                await QueryMetricSetWithFallbackAsync(
                    resourceId,
                    descriptor.ResourceType,
                    ["SuccessfulRequests", "ServerErrors", "UserErrors", "ThrottledRequests", "QuotaExceededErrors"],
                    "Total",
                    "Count",
                    null,
                    window,
                    interval,
                    armClient,
                    builder,
                    errors,
                    cancellationToken);
                break;
            case "ServiceBusQueue":
            case "ServiceBusTopic":
                await QueryMetricSetWithFallbackAsync(
                    resourceId,
                    descriptor.ResourceType,
                    ["IncomingMessages", "IncomingRequests", "ServerErrors", "UserErrors", "ThrottledRequests"],
                    "Total",
                    "Count",
                    null,
                    window,
                    interval,
                    armClient,
                    builder,
                    errors,
                    cancellationToken);
                break;
            case "StorageContainer":
                await QueryMetricSetWithFallbackAsync(
                    resourceId,
                    descriptor.ResourceType,
                    ["Transactions"],
                    "Total",
                    "Count",
                    "ResponseType eq '*'",
                    window,
                    interval,
                    armClient,
                    builder,
                    errors,
                    cancellationToken);
                break;
            case "CosmosDBSqlContainer":
                await QueryMetricSetWithFallbackAsync(
                    resourceId,
                    descriptor.ResourceType,
                    ["TotalRequests"],
                    "Count",
                    "Count",
                    "StatusCode eq '*'",
                    window,
                    interval,
                    armClient,
                    builder,
                    errors,
                    cancellationToken);
                await QueryMetricSetWithFallbackAsync(
                    resourceId,
                    descriptor.ResourceType,
                    ["NormalizedRUConsumption"],
                    "Maximum",
                    "Percent",
                    null,
                    window,
                    interval,
                    armClient,
                    builder,
                    errors,
                    cancellationToken);
                break;
        }

        var evidence = builder.Build();
        return new RoutingTargetEmitted(
            GetQueryStatus(evidence.MetricAvailability.Values.Any(status => status != "failed"), errors),
            "parentResource",
            descriptor.ResourceType,
            evidence.WindowAggregates,
            evidence.Buckets,
            errors)
        {
            MetricAvailability = evidence.MetricAvailability
        };
    }

    private async Task QueryMetricSetWithFallbackAsync(
        ResourceIdentifier resourceId,
        string resourceType,
        string[] metricNames,
        string aggregation,
        string unit,
        string? filter,
        ObservationWindow window,
        TimeSpan interval,
        ArmClient armClient,
        RoutingMetricEvidenceBuilder destination,
        List<RoutingDiagnosticError> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            await QueryMetricSetCoreAsync(
                resourceId,
                resourceType,
                metricNames,
                aggregation,
                unit,
                filter,
                window,
                interval,
                armClient,
                destination,
                errors,
                cancellationToken);
            return;
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException &&
            !cancellationToken.IsCancellationRequested &&
            metricNames.Length > 1 &&
            !IsAuthorizationError(ex) &&
            !IsNotFoundException(ex))
        {
            _logger.LogWarning(
                ex,
                "Batched target metric query failed for '{ResourceId}'; retrying metrics individually.",
                resourceId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            foreach (var metricName in metricNames.Where(name => !destination.ContainsMetric(name)))
            {
                destination.MarkFailed(metricName);
                errors.Add(CreateTargetMetricError(ex, resourceId, metricName));
            }
            return;
        }

        foreach (var metricName in metricNames.Where(name => !destination.ContainsMetric(name)))
        {
            try
            {
                await QueryMetricSetCoreAsync(
                    resourceId,
                    resourceType,
                    [metricName],
                    aggregation,
                    unit,
                    filter,
                    window,
                    interval,
                    armClient,
                    destination,
                    errors,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "Target metric '{MetricName}' is unavailable for '{ResourceId}'.",
                    metricName,
                    resourceId);
                errors.Add(CreateTargetMetricError(ex, resourceId, metricName));
                destination.MarkFailed(metricName);
            }
        }
    }

    private static async Task QueryMetricSetCoreAsync(
        ResourceIdentifier resourceId,
        string resourceType,
        string[] metricNames,
        string aggregation,
        string unit,
        string? filter,
        ObservationWindow window,
        TimeSpan interval,
        ArmClient armClient,
        RoutingMetricEvidenceBuilder builder,
        List<RoutingDiagnosticError> errors,
        CancellationToken cancellationToken)
    {
        var options = BuildMonitorMetricsOptions(
            string.Join(",", metricNames),
            resourceType,
            window,
            interval,
            aggregation,
            filter);
        var metrics = armClient.GetMonitorMetricsAsync(resourceId, options, cancellationToken);
        var returnedMetricNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await foreach (var metric in metrics.WithCancellation(cancellationToken))
        {
            var metricName = metricNames.FirstOrDefault(name =>
                string.Equals(name, metric.Name?.Value, StringComparison.OrdinalIgnoreCase));
            if (metricName is null)
            {
                continue;
            }
            returnedMetricNames.Add(metricName);
            var metricError = GetMetricResponseError(
                metric, metricName, resourceId, "targetAzureMonitor", "Monitoring Reader");
            if (metricError is not null)
            {
                builder.MarkFailed(metricName);
                errors.Add(metricError);
                continue;
            }

            var metricBuilder = new RoutingMetricEvidenceBuilder(resourceType);
            try
            {
                foreach (var series in metric.Timeseries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var dimensions = GetDimensions(series);
                    switch (aggregation)
                    {
                        case "Total":
                            metricBuilder.AddCountSeries(
                                metricName,
                                aggregation,
                                dimensions,
                                series.Data.Select(point => (point.TimeStamp, point.Total ?? point.Count)));
                            break;
                        case "Count":
                            metricBuilder.AddCountSeries(
                                metricName,
                                aggregation,
                                dimensions,
                                series.Data.Select(point => (point.TimeStamp, point.Count ?? point.Total)));
                            break;
                        case "Maximum":
                            metricBuilder.AddMaximumSeries(
                                metricName,
                                unit,
                                dimensions,
                                series.Data.Select(point => (point.TimeStamp, point.Maximum)));
                            break;
                        default:
                            throw new InvalidOperationException(
                                $"Unsupported target metric aggregation '{aggregation}'.");
                    }
                }
                metricBuilder.MarkSuccessful(metricName);
                if (metric.Timeseries.Count >= MetricSeriesLimit)
                {
                    metricBuilder.MarkPartial(metricName);
                    errors.Add(CreateSeriesLimitError(metricName, resourceId, "targetAzureMonitor"));
                }
                builder.Merge(metricBuilder.Build());
            }
            catch (Exception ex) when (ex is InvalidDataException or OverflowException)
            {
                builder.MarkFailed(metricName);
                errors.Add(CreateTargetMetricError(ex, resourceId, metricName));
            }
        }
        foreach (var metricName in metricNames.Where(name => !returnedMetricNames.Contains(name)))
        {
            builder.MarkFailed(metricName);
            errors.Add(CreateMissingMetricError(metricName, resourceId, "targetAzureMonitor"));
        }
    }

    // Monitor defaults to ten series for filtered queries; its REST/SDK top parameter is an Int32.
    internal const int MetricSeriesLimit = 10000;

    internal static ArmResourceGetMonitorMetricsOptions BuildMonitorMetricsOptions(
        string metricNames,
        string resourceType,
        ObservationWindow window,
        TimeSpan interval,
        string aggregation,
        string? filter = null)
    {
        var options = new ArmResourceGetMonitorMetricsOptions
        {
            Metricnames = metricNames,
            Metricnamespace = resourceType,
            Timespan = window.Timespan,
            Interval = interval,
            Aggregation = aggregation
        };
        if (!string.IsNullOrEmpty(filter))
        {
            options.Filter = filter;
            options.Top = MetricSeriesLimit;
        }
        return options;
    }

    private async Task<ArmClient> CreateRoutingArmClientAsync(string? tenant, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            // The shared ARM client factory can wrap credential cancellation in a general exception.
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }

    private static string BuildEndpointFilter(IReadOnlyList<RoutingEndpointDetails> endpoints)
    {
        var filter = string.Join(" or ", endpoints.Select(endpoint =>
            $"EndpointName eq '{endpoint.Name.Replace("'", "''", StringComparison.Ordinal)}'"));
        return endpoints.Count == 1 ? filter : $"({filter})";
    }

    private static RoutingDiagnosticError? GetMetricResponseError(
        MonitorMetric metric,
        string metricName,
        ResourceIdentifier resourceId,
        string source,
        string requiredRole) =>
        // Provider messages can contain resource configuration; retain the code without echoing raw text.
        (!string.IsNullOrWhiteSpace(metric.ErrorCode) &&
            !string.Equals(metric.ErrorCode, "Success", StringComparison.OrdinalIgnoreCase)) ||
        !string.IsNullOrWhiteSpace(metric.ErrorMessage)
            ? new RoutingDiagnosticError(
                source, metricName, resourceId.ToString(), 200,
                string.IsNullOrWhiteSpace(metric.ErrorCode) ||
                    string.Equals(metric.ErrorCode, "Success", StringComparison.OrdinalIgnoreCase)
                        ? "MetricQueryFailed" : metric.ErrorCode,
                requiredRole,
                $"Azure Monitor reported an error for metric '{metricName}' in a successful HTTP response.")
            : null;

    private static RoutingDiagnosticError CreateSeriesLimitError(
        string metricName,
        ResourceIdentifier resourceId,
        string source) => new(
            source, metricName, resourceId.ToString(), 200, "PossibleTruncation", null,
            $"Metric '{metricName}' reached the requested limit of {MetricSeriesLimit} time series. " +
            "Returned buckets and window aggregates may be incomplete; missing values must not be treated as zero.");

    private static RoutingDiagnosticError CreateMissingMetricError(
        string metricName,
        ResourceIdentifier resourceId,
        string source) => new(
            source, metricName, resourceId.ToString(), 200, "MissingMetricResponse", null,
            $"Azure Monitor did not return the requested metric '{metricName}'. " +
            "Its availability cannot be determined from this response.");

    private async Task<ExistenceResult> GetTargetExistenceAsync(
        ResourceIdentifier resourceId,
        string apiVersion,
        string endpointName,
        string? tenant,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await GetArmAsync(
                resourceId.ToString(),
                apiVersion,
                tenant,
                cancellationToken);
            var observedAt = DateTimeOffset.UtcNow;
            if (response.IsSuccess)
            {
                return new ExistenceResult("exists", observedAt, null);
            }
            if (IsResourceNotFoundResponse(response.StatusCode, response.Content))
            {
                return new ExistenceResult("notFound", observedAt, null);
            }

            return new ExistenceResult(
                "indeterminate",
                observedAt,
                new RoutingDiagnosticError(
                    "targetArm",
                    "read",
                    resourceId.ToString(),
                    (int)response.StatusCode,
                    GetArmErrorCode(response.Content) ?? "Indeterminate",
                    "Reader",
                    "The current target resource existence could not be determined."));
        }
        catch (Exception ex) when (ex is not OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Unable to check routed resource existence for endpoint '{EndpointName}'.",
                endpointName);
            return new ExistenceResult(
                "indeterminate",
                DateTimeOffset.UtcNow,
                CreateDiagnosticError(
                    ex,
                    "targetArm",
                    "read",
                    resourceId.ToString(),
                    "Reader",
                    "The current target resource existence could not be determined."));
        }
    }

    internal async Task<List<EndpointHealthData>> GetRoutingEndpointsHealthAsync(
        ResourceIdentifier hubResourceId,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var results = new List<EndpointHealthData>();
        string? nextLink = $"{hubResourceId}/routingEndpointsHealth";
        string? apiVersion = "2023-06-30";
        while (!string.IsNullOrEmpty(nextLink))
        {
            var response = await GetArmAsync(nextLink, apiVersion, tenant, cancellationToken);
            if (!response.IsSuccess)
            {
                throw new RequestFailedException(
                    (int)response.StatusCode,
                    "IoT Hub routing endpoint health could not be read.");
            }

            var page = JsonSerializer.Deserialize(
                response.Content,
                IoTHubJsonContext.Default.EndpointHealthDataListResult);
            results.AddRange(page?.Value ?? []);
            nextLink = page?.NextLink;
            apiVersion = null;
        }
        return results;
    }

    private async Task<ArmGetResult> GetArmAsync(
        string resourcePathOrUrl,
        string? apiVersion,
        string? tenant,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        TokenCredential credential;
        try
        {
            var tenantId = string.IsNullOrEmpty(tenant)
                ? null
                : await AzureService.ResolveTenantIdAsync(tenant, cancellationToken);
            credential = await AzureService.GetTokenCredentialAsync(tenantId, cancellationToken);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
        var requestUri = BuildArmRequestUri(
            AzureService.CloudConfiguration.ArmEnvironment,
            resourcePathOrUrl,
            apiVersion);

        using var httpClient = _httpClientFactory.CreateClient();
        using var transport = new HttpClientTransport(httpClient);
        var options = AddDefaultPolicies(new ArmClientOptions());
        options.Transport = transport;
        options.AddPolicy(
            new BearerTokenAuthenticationPolicy(
                credential, AzureService.CloudConfiguration.ArmEnvironment.DefaultScope),
            HttpPipelinePosition.PerRetry);
        var pipeline = HttpPipelineBuilder.Build(options);
        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Get;
        request.Uri.Reset(requestUri);
        request.Headers.Add("Accept", "application/json");
        using var response = await pipeline.SendRequestAsync(request, cancellationToken);
        return new ArmGetResult(
            (HttpStatusCode)response.Status,
            response.Content.ToString());
    }

    internal static Uri BuildArmRequestUri(
        ArmEnvironment armEnvironment,
        string resourcePathOrNextLink,
        string? apiVersion)
    {
        // Classify rooted ARM paths first: on Linux and macOS, Uri.TryCreate parses "/subscriptions/..."
        // as an absolute file:// URI.
        var url = resourcePathOrNextLink.StartsWith('/')
            ? $"{armEnvironment.Endpoint.AbsoluteUri.TrimEnd('/')}{resourcePathOrNextLink}"
            : resourcePathOrNextLink;
        if (!string.IsNullOrEmpty(apiVersion))
        {
            url += $"{(url.Contains('?') ? '&' : '?')}api-version={apiVersion}";
        }

        // Initial requests and ARM continuation links must both target the configured cloud's ARM host
        // before the bearer token is attached.
        EndpointValidator.ValidateAzureServiceEndpoint(
            endpoint: url,
            serviceType: "arm",
            armEnvironment: armEnvironment,
            executingToolNamespaceName: "iothub");
        return new Uri(url);
    }

    internal static bool IsResourceNotFoundResponse(
        HttpStatusCode statusCode,
        string? responseContent)
    {
        if (statusCode == HttpStatusCode.NotFound)
        {
            return true;
        }
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return false;
        }

        try
        {
            return IsResourceNotFoundErrorCode(GetArmErrorCode(responseContent));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? GetArmErrorCode(string? responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return null;
        }

        using var document = JsonDocument.Parse(responseContent);
        return document.RootElement.TryGetProperty("error", out var error)
            && error.TryGetProperty("code", out var code)
                ? code.GetString()
                : null;
    }

    private static bool IsResourceNotFoundErrorCode(string? errorCode) =>
        errorCode is not null
        && (errorCode.Equals("ResourceNotFound", StringComparison.OrdinalIgnoreCase)
            || errorCode.Equals("ParentResourceNotFound", StringComparison.OrdinalIgnoreCase)
            || errorCode.Equals("EntityNotFound", StringComparison.OrdinalIgnoreCase)
            || errorCode.Equals("MessagingEntityNotFound", StringComparison.OrdinalIgnoreCase)
            || errorCode.Equals("ContainerNotFound", StringComparison.OrdinalIgnoreCase)
            || errorCode.Equals("NotFound", StringComparison.OrdinalIgnoreCase));

    private async Task<GenericResourceData> GetIoTHubGenericResourceAsync(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscriptionResource = await AzureService.GetSubscription(
                subscription,
                tenant,
                cancellationToken: cancellationToken);
            var armClient = await CreateRoutingArmClientAsync(tenant, cancellationToken);
            var resourceId = BuildResourceId(
                subscriptionResource.Data.SubscriptionId,
                resourceGroup,
                "Microsoft.Devices/IotHubs",
                hubName);
            var hub = await armClient.GetGenericResource(resourceId).GetAsync(cancellationToken);
            return hub.Value.Data;
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            // Subscription resolution can also wrap cancellation from its ARM client factory.
            cancellationToken.ThrowIfCancellationRequested();
            throw;
        }
    }

    private static List<RoutingEndpointDetails> FilterEndpoints(
        List<RoutingEndpointDetails> endpoints,
        string? endpointName,
        string hubName)
    {
        if (string.IsNullOrWhiteSpace(endpointName))
        {
            return endpoints;
        }

        var filtered = endpoints
            .Where(endpoint =>
                string.Equals(endpoint.Name, endpointName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return filtered.Count > 0
            ? filtered
            : throw new KeyNotFoundException(
                $"Routing endpoint '{endpointName}' not found on IoT Hub '{hubName}'.");
    }

    private static ResolvedTarget? ResolveTarget(RoutingEndpointDetails endpoint)
    {
        var descriptor = GetTargetDescriptor(endpoint.EndpointType);
        if (descriptor is null ||
            string.IsNullOrWhiteSpace(endpoint.SubscriptionId) ||
            string.IsNullOrWhiteSpace(endpoint.ResourceGroup) ||
            string.IsNullOrWhiteSpace(endpoint.EndpointResourceName))
        {
            return null;
        }

        var parent = BuildResourceId(
            endpoint.SubscriptionId,
            endpoint.ResourceGroup,
            descriptor.ResourceType,
            endpoint.EndpointResourceName);
        ResourceIdentifier? routedResource = endpoint.EndpointType switch
        {
            "EventHub" when !string.IsNullOrWhiteSpace(endpoint.EntityPath)
                => new ResourceIdentifier($"{parent}/eventhubs/{endpoint.EntityPath}"),
            "ServiceBusQueue" when !string.IsNullOrWhiteSpace(endpoint.EntityPath)
                => new ResourceIdentifier($"{parent}/queues/{endpoint.EntityPath}"),
            "ServiceBusTopic" when !string.IsNullOrWhiteSpace(endpoint.EntityPath)
                => new ResourceIdentifier($"{parent}/topics/{endpoint.EntityPath}"),
            "StorageContainer" when !string.IsNullOrWhiteSpace(endpoint.ContainerName)
                => new ResourceIdentifier(
                    $"{parent}/blobServices/default/containers/{endpoint.ContainerName}"),
            "CosmosDBSqlContainer" when
                !string.IsNullOrWhiteSpace(endpoint.DatabaseName) &&
                !string.IsNullOrWhiteSpace(endpoint.ContainerName)
                => new ResourceIdentifier(
                    $"{parent}/sqlDatabases/{endpoint.DatabaseName}/containers/{endpoint.ContainerName}"),
            _ => null
        };
        return routedResource is null ? null : new ResolvedTarget(descriptor, parent, routedResource);
    }

    private static RoutingTargetDescriptor? GetTargetDescriptor(string endpointType) => endpointType switch
    {
        "EventHub" => new("EventHub", "Microsoft.EventHub/namespaces", "2024-01-01"),
        "ServiceBusQueue" => new("ServiceBusQueue", "Microsoft.ServiceBus/namespaces", "2024-01-01"),
        "ServiceBusTopic" => new("ServiceBusTopic", "Microsoft.ServiceBus/namespaces", "2024-01-01"),
        "StorageContainer" => new("StorageContainer", "Microsoft.Storage/storageAccounts", "2023-05-01"),
        "CosmosDBSqlContainer" => new("CosmosDBSqlContainer", "Microsoft.DocumentDB/databaseAccounts", "2024-05-15"),
        _ => null
    };

    private static IReadOnlyDictionary<string, string> GetDimensions(
        MonitorTimeSeriesElement series)
    {
        var dimensions = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var metadata in series.Metadatavalues)
        {
            var name = metadata.Name?.Value;
            if (!string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(metadata.Value) &&
                !string.Equals(metadata.Value, "__Empty", StringComparison.OrdinalIgnoreCase))
            {
                dimensions[name] = metadata.Value;
            }
        }
        return dimensions;
    }

    private static string? GetDimensionValue(MonitorTimeSeriesElement series, string name) =>
        series.Metadatavalues
            .FirstOrDefault(metadata =>
                string.Equals(metadata.Name?.Value, name, StringComparison.OrdinalIgnoreCase))
            ?.Value;

    private static void AddDimensionIfPresent(
        MonitorTimeSeriesElement series,
        Dictionary<string, string> dimensions,
        string name)
    {
        var value = GetDimensionValue(series, name);
        if (!string.IsNullOrWhiteSpace(value) &&
            !string.Equals(value, "__Empty", StringComparison.OrdinalIgnoreCase))
        {
            dimensions[name] = value;
        }
    }

    private static void AddSharedError(
        Dictionary<string, List<RoutingDiagnosticError>> errors,
        RoutingDiagnosticError error)
    {
        foreach (var endpointErrors in errors.Values)
        {
            endpointErrors.Add(error);
        }
    }

    private static RoutingDiagnosticError CreateTargetMetricError(
        Exception exception,
        ResourceIdentifier resourceId,
        string metricName) => CreateDiagnosticError(
            exception,
            "targetAzureMonitor",
            metricName,
            resourceId.ToString(),
            "Monitoring Reader",
            $"Target Azure Monitor metric '{metricName}' could not be read.");

    private static RoutingDiagnosticError CreateDiagnosticError(
        Exception exception,
        string source,
        string operation,
        string? resourceId,
        string? requiredRole,
        string message)
    {
        var requestFailed = exception as RequestFailedException;
        return new RoutingDiagnosticError(
            source,
            operation,
            resourceId,
            requestFailed?.Status,
            requestFailed?.ErrorCode ?? exception.GetType().Name,
            requiredRole,
            message);
    }

    private static string GetQueryStatus(
        bool hasSuccessfulQueries,
        IReadOnlyList<RoutingDiagnosticError> errors)
    {
        if (errors.Count == 0)
        {
            return "queried";
        }
        if (hasSuccessfulQueries)
        {
            return "partial";
        }
        if (errors.Any(error => error.StatusCode is 401 or 403))
        {
            return "unauthorized";
        }
        return errors.Any(error => error.StatusCode == 404) ? "notFound" : "failed";
    }

    private static RoutingHubEmitted CreateEmptyHubEvidence() => new(
        new RoutingMetricEvidence("Microsoft.Devices/IotHubs", [], []),
        []);

    private static RoutingTargetEmitted CreateEmptyTargetEvidence(string status) => new(
        status,
        "parentResource",
        null,
        [],
        [],
        []);

    private static bool IsAuthorizationError(Exception exception) =>
        exception is RequestFailedException requestFailed &&
            requestFailed.Status is 401 or 403
        || exception is Azure.Identity.AuthenticationFailedException
        || exception.Message.Contains("AuthorizationFailed", StringComparison.OrdinalIgnoreCase);

    private static bool IsNotFoundException(Exception exception) =>
        exception is RequestFailedException requestFailed && requestFailed.Status == 404;

    private static void ValidateInputs(
        string hubName,
        string resourceGroup,
        string subscription) =>
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(hubName), hubName));

    private static ResourceIdentifier BuildResourceId(
        string subscription,
        string resourceGroup,
        string resourceType,
        string resourceName) =>
        new($"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{resourceName}");

    private static string ToIsoString(DateTimeOffset value) =>
        value.ToUniversalTime().ToString(
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            CultureInfo.InvariantCulture);

    private static readonly IReadOnlyDictionary<string, string> EmptyDimensions =
        new Dictionary<string, string>();
}
