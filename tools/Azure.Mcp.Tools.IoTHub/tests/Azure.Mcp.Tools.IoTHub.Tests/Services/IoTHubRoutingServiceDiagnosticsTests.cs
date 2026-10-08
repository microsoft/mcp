// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Services;

public class IoTHubRoutingServiceDiagnosticsTests()
{
    private const string Subscription = "11111111-1111-1111-1111-111111111111";
    private const string HubId = $"/subscriptions/{Subscription}/resourceGroups/rg1/providers/Microsoft.Devices/IotHubs/hub1";
    private static readonly DateTimeOffset s_startTime = new(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Diagnostics_ReturnsMoreThanTenSeriesAndFiltersSelectedEndpoint()
    {
        using var handler = CreateHandler((request, _) =>
        {
            var name = GetQuery(request, "metricnames");
            var top = int.TryParse(GetQuery(request, "top"), out var value) ? value : 10;
            return name switch
            {
                "RoutingDeliveries" => Metrics(Metric(name, Series(Math.Min(top, 15), true))),
                "Transactions" => Metrics(Metric(name, Series(Math.Min(top, 15), false))),
                _ => Metrics(Metric(name))
            };
        });

        var result = await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken, endpointName: "endpoint1");

        var endpoint = Assert.Single(result.Endpoints);
        Assert.Equal(15, endpoint.HubEmitted.RoutingMetrics.WindowAggregates.Count);
        Assert.Equal(15, endpoint.TargetEmitted.WindowAggregates.Count);
        Assert.Equal("valuesReturned", endpoint.HubEmitted.RoutingMetrics.MetricAvailability["RoutingDeliveries"]);
        Assert.Equal("valuesReturned", endpoint.TargetEmitted.MetricAvailability["Transactions"]);
        Assert.Empty(endpoint.HubEmitted.Errors);
        Assert.Empty(endpoint.TargetEmitted.Errors);
        var hubQueries = handler.RequestUris.Where(uri => uri.AbsolutePath.Contains("/IotHubs/", StringComparison.Ordinal)
            && uri.AbsolutePath.EndsWith("/metrics", StringComparison.Ordinal)).ToArray();
        Assert.Equal(2, hubQueries.Length);
        Assert.All(hubQueries, uri =>
        {
            var filter = GetQuery(uri, "$filter");
            Assert.Contains("EndpointName eq 'endpoint1'", filter);
            Assert.DoesNotContain("EndpointName eq '*'", filter);
            Assert.Equal("10000", GetQuery(uri, "top"));
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Diagnostics_SignalsPossibleTruncationAtSeriesLimit(bool hubMetric)
    {
        using var handler = CreateHandler((request, _) =>
        {
            var name = GetQuery(request, "metricnames");
            var selected = hubMetric ? "RoutingDeliveries" : "Transactions";
            return Metrics(Metric(name, name == selected
                ? Series(IoTHubRoutingService.MetricSeriesLimit, hubMetric, distinctDimensions: false)
                : ""));
        });

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        var availability = hubMetric
            ? endpoint.HubEmitted.RoutingMetrics.MetricAvailability
            : endpoint.TargetEmitted.MetricAvailability;
        var errors = hubMetric ? endpoint.HubEmitted.Errors : endpoint.TargetEmitted.Errors;
        Assert.Equal("partial", availability[hubMetric ? "RoutingDeliveries" : "Transactions"]);
        Assert.Equal("PossibleTruncation", Assert.Single(errors).Code);
        if (!hubMetric)
        {
            Assert.Equal("partial", endpoint.TargetEmitted.QueryStatus);
        }
    }

    [Fact]
    public async Task Diagnostics_ReportsEmbeddedHubErrorWithoutHidingOtherMetric()
    {
        using var handler = CreateHandler((request, _) =>
        {
            var name = GetQuery(request, "metricnames");
            return Metrics(name == "RoutingDeliveries"
                ? Metric(name, errorCode: "InvalidSamplingType", errorMessage: "Unsupported aggregation")
                : name == "RoutingDeliveryLatency"
                    ? Metric(name, Series(1, true, aggregation: "average"))
                    : Metric(name));
        });

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("failed", endpoint.HubEmitted.RoutingMetrics.MetricAvailability["RoutingDeliveries"]);
        Assert.Equal("valuesReturned", endpoint.HubEmitted.RoutingMetrics.MetricAvailability["RoutingDeliveryLatency"]);
        Assert.Single(endpoint.HubEmitted.RoutingMetrics.Buckets);
        var error = Assert.Single(endpoint.HubEmitted.Errors);
        Assert.Equal("InvalidSamplingType", error.Code);
        Assert.Equal(200, error.StatusCode);
        Assert.Equal("RoutingDeliveries", error.Operation);
    }

    [Theory]
    [InlineData("BadArgument", "Unsupported metric", "BadArgument")]
    [InlineData("", "Metric is unavailable", "MetricQueryFailed")]
    [InlineData("BadArgument", "SharedAccessKey=fake-secret", "BadArgument")]
    public async Task Diagnostics_PreservesMixedTargetBatchIncludingEmptySuccess(
        string errorCode, string errorMessage, string expectedCode)
    {
        using var handler = CreateHandler((request, _) =>
        {
            var name = GetQuery(request, "metricnames");
            return IsHubMetric(request)
                ? Metrics(Metric(name))
                : Metrics(
                    Metric("SuccessfulRequests", Series(1, false)),
                    Metric("ServerErrors", errorCode: errorCode, errorMessage: errorMessage),
                    Metric("UserErrors"),
                    Metric("ThrottledRequests"),
                    Metric("QuotaExceededErrors"));
        }, eventHub: true);

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("partial", endpoint.TargetEmitted.QueryStatus);
        Assert.Single(endpoint.TargetEmitted.WindowAggregates);
        Assert.Equal("valuesReturned", endpoint.TargetEmitted.MetricAvailability["SuccessfulRequests"]);
        Assert.Equal("failed", endpoint.TargetEmitted.MetricAvailability["ServerErrors"]);
        Assert.Equal("noValuesReturned", endpoint.TargetEmitted.MetricAvailability["UserErrors"]);
        var error = Assert.Single(endpoint.TargetEmitted.Errors);
        Assert.Equal(expectedCode, error.Code);
        Assert.DoesNotContain(errorMessage, error.Message);
        Assert.Single(TargetMetricRequests(handler));
    }

    [Fact]
    public async Task Diagnostics_EmptySuccessWithFailedMetricIsPartialNotFailed()
    {
        using var handler = CreateHandler((request, _) => IsHubMetric(request)
            ? Metrics(Metric(GetQuery(request, "metricnames")))
            : Metrics(
                Metric("SuccessfulRequests"),
                Metric("ServerErrors", errorCode: "BadArgument"),
                Metric("UserErrors", errorCode: "BadArgument"),
                Metric("ThrottledRequests", errorCode: "BadArgument"),
                Metric("QuotaExceededErrors", errorCode: "BadArgument")), eventHub: true);

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("partial", endpoint.TargetEmitted.QueryStatus);
        Assert.Empty(endpoint.TargetEmitted.WindowAggregates);
        Assert.Empty(endpoint.TargetEmitted.Buckets);
        Assert.Equal("noValuesReturned", endpoint.TargetEmitted.MetricAvailability["SuccessfulRequests"]);
        Assert.Equal(4, endpoint.TargetEmitted.Errors.Count);
    }

    [Fact]
    public async Task Diagnostics_AllEmbeddedTargetErrorsAreFailedNotEmptySuccess()
    {
        using var handler = CreateHandler((request, _) => Metrics(Metric(
            GetQuery(request, "metricnames"), errorCode: IsHubMetric(request) ? "Success" : "BadArgument")));

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("failed", endpoint.TargetEmitted.QueryStatus);
        Assert.Equal("failed", endpoint.TargetEmitted.MetricAvailability["Transactions"]);
        Assert.Single(endpoint.TargetEmitted.Errors);
    }

    [Fact]
    public async Task Diagnostics_InvalidMetricValuesDoNotDiscardValidBatchMetrics()
    {
        using var handler = CreateHandler((request, _) => IsHubMetric(request)
            ? Metrics()
            : Metrics(
                Metric("SuccessfulRequests", Series(1, false, value: "3")),
                Metric("ServerErrors", Series(1, false, value: "0.5")),
                Metric("UserErrors"),
                Metric("ThrottledRequests"),
                Metric("QuotaExceededErrors")), eventHub: true);

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("partial", endpoint.TargetEmitted.QueryStatus);
        Assert.Equal(3, Assert.Single(endpoint.TargetEmitted.WindowAggregates).Value.GetInt64());
        Assert.Equal("failed", endpoint.TargetEmitted.MetricAvailability["ServerErrors"]);
        Assert.Single(endpoint.TargetEmitted.Errors);
        Assert.Single(TargetMetricRequests(handler));
    }

    [Fact]
    public async Task Diagnostics_EmptyMetricListReportsMissingMetrics()
    {
        using var handler = CreateHandler((_, _) => Metrics());

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("failed", endpoint.TargetEmitted.QueryStatus);
        Assert.Equal("failed", endpoint.TargetEmitted.MetricAvailability["Transactions"]);
        Assert.Equal("MissingMetricResponse", Assert.Single(endpoint.TargetEmitted.Errors).Code);
        Assert.Equal(2, endpoint.HubEmitted.RoutingMetrics.MetricAvailability.Count);
        Assert.All(endpoint.HubEmitted.RoutingMetrics.MetricAvailability.Values,
            status => Assert.Equal("failed", status));
        Assert.Equal(2, endpoint.HubEmitted.Errors.Count);
        Assert.All(endpoint.HubEmitted.Errors, error => Assert.Equal("MissingMetricResponse", error.Code));
    }

    [Fact]
    public async Task Diagnostics_MissingBatchMetricPreservesReturnedValuesAndEmptySuccess()
    {
        using var handler = CreateHandler((request, _) => IsHubMetric(request)
            ? Metrics(Metric(GetQuery(request, "metricnames")))
            : Metrics(
                Metric("SuccessfulRequests", Series(1, false)),
                Metric("ServerErrors"),
                Metric("UserErrors"),
                Metric("QuotaExceededErrors")), eventHub: true);

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("partial", endpoint.TargetEmitted.QueryStatus);
        Assert.Single(endpoint.TargetEmitted.WindowAggregates);
        Assert.Equal("valuesReturned", endpoint.TargetEmitted.MetricAvailability["SuccessfulRequests"]);
        Assert.Equal("noValuesReturned", endpoint.TargetEmitted.MetricAvailability["ServerErrors"]);
        Assert.Equal("failed", endpoint.TargetEmitted.MetricAvailability["ThrottledRequests"]);
        var error = Assert.Single(endpoint.TargetEmitted.Errors);
        Assert.Equal("MissingMetricResponse", error.Code);
        Assert.Equal("ThrottledRequests", error.Operation);
        Assert.Empty(endpoint.HubEmitted.Errors);
        Assert.Single(TargetMetricRequests(handler));
    }

    [Theory]
    [InlineData("")]
    [InlineData("UnexpectedMetric")]
    public async Task Diagnostics_UnmatchedMetricObjectsDoNotImplyRequestedMetricSuccess(string responseMetricName)
    {
        using var handler = CreateHandler((_, _) => Metrics(Metric(responseMetricName)));

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("failed", endpoint.TargetEmitted.QueryStatus);
        Assert.Equal("failed", endpoint.TargetEmitted.MetricAvailability["Transactions"]);
        Assert.Equal("MissingMetricResponse", Assert.Single(endpoint.TargetEmitted.Errors).Code);
        Assert.Equal(2, endpoint.HubEmitted.Errors.Count);
        Assert.All(endpoint.HubEmitted.Errors, error => Assert.Equal("MissingMetricResponse", error.Code));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Diagnostics_EmptySuccessfulResponsesDoNotInventZeroes(bool nullValues)
    {
        using var handler = CreateHandler((request, _) => Metrics(Metric(
            GetQuery(request, "metricnames"),
            nullValues ? Series(1, IsHubMetric(request), value: "null") : "")));

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("queried", endpoint.TargetEmitted.QueryStatus);
        Assert.All(endpoint.HubEmitted.RoutingMetrics.MetricAvailability.Values,
            status => Assert.Equal("noValuesReturned", status));
        Assert.Equal("noValuesReturned", endpoint.TargetEmitted.MetricAvailability["Transactions"]);
        Assert.Empty(endpoint.HubEmitted.RoutingMetrics.Buckets);
        Assert.Empty(endpoint.HubEmitted.RoutingMetrics.WindowAggregates);
        Assert.Empty(endpoint.TargetEmitted.Buckets);
        Assert.Empty(endpoint.TargetEmitted.WindowAggregates);
        Assert.Empty(endpoint.TargetEmitted.Errors);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task RoutingOperations_PropagateCancellationDuringHubLookup(
        bool health, bool credentialStage)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = CreateHandler((_, _) => Metrics());
        var azureService = Substitute.For<IAzureService>();
        var service = CreateService(handler, azureService);
        if (credentialStage)
        {
            azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    cancellation.Cancel();
                    return Task.FromCanceled<TokenCredential>(cancellation.Token);
                });
        }
        else
        {
            azureService.GetSubscription(Subscription, null, Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    cancellation.Cancel();
                    return Task.FromCanceled<SubscriptionResource>(cancellation.Token);
                });
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            if (health)
            {
                await service.GetRoutingEndpointHealth(
                    "hub1", "rg1", Subscription, cancellationToken: cancellation.Token);
            }
            else
            {
                await DiagnoseAsync(service, cancellation.Token);
            }
        });
        Assert.Empty(handler.RequestUris);
    }

    [Theory]
    [InlineData("RoutingDeliveries")]
    [InlineData("RoutingDeliveryLatency")]
    [InlineData("targetMetrics")]
    [InlineData("existence")]
    public async Task Diagnostics_PropagatesCancellationWithoutFallback(string stage)
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = CreateHandler((request, _) =>
        {
            var name = GetQuery(request, "metricnames");
            if (name == stage || (stage == "targetMetrics" && !IsHubMetric(request)))
            {
                cancellation.Cancel();
                cancellation.Token.ThrowIfCancellationRequested();
            }
            return Metrics();
        }, eventHub: true, existenceResponse: (_, _) =>
        {
            if (stage == "existence")
            {
                cancellation.Cancel();
                cancellation.Token.ThrowIfCancellationRequested();
            }
            return JsonResponse("{}");
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            DiagnoseAsync(CreateService(handler), cancellationToken: cancellation.Token));

        Assert.True(TargetMetricRequests(handler).Count() <= 1);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "exists")]
    [InlineData(HttpStatusCode.NotFound, "notFound")]
    [InlineData(HttpStatusCode.Forbidden, "indeterminate")]
    public async Task Diagnostics_QueriesRoutedResourceWithArmAuthentication(
        HttpStatusCode statusCode, string expected)
    {
        var requests = 0;
        using var handler = CreateHandler(
            (request, _) => Metrics(Metric(GetQuery(request, "metricnames"))),
            existenceResponse: (request, _) =>
            {
                requests++;
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal(
                    $"https://management.azure.com/subscriptions/{Subscription}/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/shared/blobServices/default/containers/container1?api-version=2023-05-01",
                    request.RequestUri!.ToString());
                Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
                Assert.Equal("fake-token", request.Headers.Authorization?.Parameter);
                return JsonResponse("""{"error":{"code":"AuthorizationFailed"}}""", statusCode);
            });

        var endpoint = Assert.Single((await DiagnoseAsync(
            CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal(expected, endpoint.Target.ExistenceStatus);
        Assert.Equal(1, requests);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Diagnostics_RetriesTransientExistenceFailures(HttpStatusCode statusCode)
    {
        var attempts = 0;
        using var handler = CreateHandler(
            (request, _) => Metrics(Metric(GetQuery(request, "metricnames"))),
            existenceResponse: (_, _) =>
            {
                var response = ++attempts == 1
                    ? JsonResponse("""{"error":{"code":"ServerBusy"}}""", statusCode)
                    : JsonResponse("{}");
                response.Headers.Add("x-ms-retry-after-ms", "1");
                return response;
            });

        var endpoint = Assert.Single((await DiagnoseAsync(
            CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("exists", endpoint.Target.ExistenceStatus);
        Assert.Null(endpoint.Target.Error);
        Assert.Equal(2, attempts);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "notFound")]
    [InlineData(HttpStatusCode.Forbidden, "indeterminate")]
    public async Task Diagnostics_SeparatesExistenceFromSharedParentMetrics(
        HttpStatusCode existenceCode, string existenceStatus)
    {
        var before = DateTimeOffset.UtcNow;
        using var handler = CreateHandler(
            (request, _) => Metrics(Metric(GetQuery(request, "metricnames"))),
            endpointCount: 2,
            existenceResponse: (request, _) => request.RequestUri!.AbsolutePath.EndsWith("/container1", StringComparison.Ordinal)
                ? JsonResponse("""{"error":{"code":"AuthorizationFailed"}}""", existenceCode)
                : JsonResponse("{}"));

        var result = await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Endpoints.Count);
        var first = result.Endpoints[0];
        var second = result.Endpoints[1];
        Assert.Equal(existenceStatus, first.Target.ExistenceStatus);
        Assert.Equal("exists", second.Target.ExistenceStatus);
        Assert.InRange(DateTimeOffset.Parse(first.Target.ExistenceObservedAt!), before, DateTimeOffset.UtcNow);
        Assert.Equal("parentResource", first.TargetEmitted.MetricScope);
        Assert.Equal("queried", first.TargetEmitted.QueryStatus);
        Assert.Empty(first.TargetEmitted.Errors);
        Assert.Same(first.TargetEmitted, second.TargetEmitted);
        Assert.Single(TargetMetricRequests(handler));
        Assert.Null(second.Target.Error);
        if (existenceCode == HttpStatusCode.Forbidden)
        {
            Assert.NotNull(first.Target.Error);
            Assert.Equal("targetArm", first.Target.Error.Source);
            Assert.Equal(403, first.Target.Error.StatusCode);
        }
        else
        {
            Assert.Null(first.Target.Error);
        }
    }

    [Fact]
    public async Task Diagnostics_ReportsIndeterminateExistenceAfterDefaultRetriesAreExhausted()
    {
        var attempts = 0;
        using var handler = CreateHandler(
            (request, _) => Metrics(Metric(GetQuery(request, "metricnames"))),
            existenceResponse: (_, _) =>
            {
                attempts++;
                var response = JsonResponse(
                    """{"error":{"code":"ServerBusy"}}""", HttpStatusCode.ServiceUnavailable);
                response.Headers.Add("x-ms-retry-after-ms", "1");
                return response;
            });

        var endpoint = Assert.Single((await DiagnoseAsync(
            CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal(new ArmClientOptions().Retry.MaxRetries + 1, attempts);
        Assert.Equal("indeterminate", endpoint.Target.ExistenceStatus);
        Assert.NotNull(endpoint.Target.Error);
        Assert.Equal(503, endpoint.Target.Error.StatusCode);
        Assert.Equal("ServerBusy", endpoint.Target.Error.Code);
        Assert.Equal("queried", endpoint.TargetEmitted.QueryStatus);
    }

    [Fact]
    public async Task Diagnostics_FallsBackAfterBatchRequestFailureAndKeepsEmptySuccess()
    {
        using var handler = CreateHandler((request, _) =>
        {
            var name = GetQuery(request, "metricnames");
            if (!IsHubMetric(request) && (name.Contains(',') || name == "ServerErrors"))
            {
                return JsonResponse("""{"error":{"code":"BadRequest","message":"Unsupported metric"}}""",
                    HttpStatusCode.BadRequest);
            }
            return Metrics(Metric(name));
        }, eventHub: true);

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal("partial", endpoint.TargetEmitted.QueryStatus);
        Assert.Equal(6, TargetMetricRequests(handler).Count());
        Assert.Equal("failed", endpoint.TargetEmitted.MetricAvailability["ServerErrors"]);
        Assert.Equal("noValuesReturned", endpoint.TargetEmitted.MetricAvailability["SuccessfulRequests"]);
        Assert.Single(endpoint.TargetEmitted.Errors);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Diagnostics_DoesNotFallBackAfterTransientBatchFailure(HttpStatusCode statusCode)
    {
        using var handler = CreateHandler((request, _) =>
        {
            if (IsHubMetric(request))
            {
                return Metrics(Metric(GetQuery(request, "metricnames")));
            }
            var response = JsonResponse("""{"error":{"code":"ServerBusy"}}""", statusCode);
            response.Headers.Add("x-ms-retry-after-ms", "1");
            return response;
        }, eventHub: true);

        var endpoint = Assert.Single((await DiagnoseAsync(CreateService(handler), TestContext.Current.CancellationToken)).Endpoints);

        Assert.Equal(new ArmClientOptions().Retry.MaxRetries + 1, TargetMetricRequests(handler).Count());
        Assert.All(TargetMetricRequests(handler), uri => Assert.Contains(',', GetQuery(uri, "metricnames")));
        Assert.Equal(5, endpoint.TargetEmitted.MetricAvailability.Count);
        Assert.All(endpoint.TargetEmitted.MetricAvailability.Values, status => Assert.Equal("failed", status));
        Assert.Equal("failed", endpoint.TargetEmitted.QueryStatus);
    }

    [Fact]
    public async Task Diagnostics_CancellationDuringFallbackStopsRemainingQueries()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = CreateHandler((request, _) =>
        {
            if (IsHubMetric(request))
            {
                return Metrics();
            }
            if (GetQuery(request, "metricnames").Contains(','))
            {
                return JsonResponse("""{"error":{"code":"BadRequest"}}""", HttpStatusCode.BadRequest);
            }
            cancellation.Cancel();
            cancellation.Token.ThrowIfCancellationRequested();
            return Metrics();
        }, eventHub: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            DiagnoseAsync(CreateService(handler), cancellationToken: cancellation.Token));

        Assert.Equal(2, TargetMetricRequests(handler).Count());
    }

    private static Task<RoutingEndpointDiagnostics> DiagnoseAsync(
        IoTHubRoutingService service,
        CancellationToken cancellationToken,
        string? endpointName = null) =>
        service.GetRoutingEndpointDiagnostics(
            "hub1", "rg1", Subscription, endpointName, s_startTime, s_startTime.AddHours(1), "PT1H",
            cancellationToken: cancellationToken);

    private static IEnumerable<Uri> TargetMetricRequests(RoutingDiagnosticsHttpMessageHandler handler) =>
        handler.RequestUris.Where(uri => uri.AbsolutePath.EndsWith("/metrics", StringComparison.Ordinal)
            && !uri.AbsolutePath.Contains("/IotHubs/", StringComparison.Ordinal));

    private static bool IsHubMetric(HttpRequestMessage request) =>
        request.RequestUri!.AbsolutePath.Contains("/IotHubs/", StringComparison.Ordinal);

    private static string GetQuery(HttpRequestMessage request, string key) => GetQuery(request.RequestUri!, key);

    private static string GetQuery(Uri uri, string key) =>
        uri.Query.TrimStart('?').Split('&')
            .Select(pair => pair.Split('=', 2))
            .Where(pair => Uri.UnescapeDataString(pair[0]) == key)
            .Select(pair => Uri.UnescapeDataString(pair[1]))
            .SingleOrDefault() ?? "";

    private static HttpResponseMessage Metrics(params string[] metrics) =>
        JsonResponse($$"""{"timespan":"2026-08-01T00:00:00Z/2026-08-01T01:00:00Z","interval":"PT1H","value":[{{string.Join(",", metrics)}}]}""");

    private static string Metric(string name, string series = "", string errorCode = "Success", string errorMessage = "") =>
        $$"""{"id":"{{HubId}}/providers/Microsoft.Insights/metrics/{{name}}","type":"Microsoft.Insights/metrics","name":{"value":"{{name}}"},"unit":"Count","timeseries":[{{series}}],"errorCode":"{{errorCode}}","errorMessage":"{{errorMessage}}"}""";

    private static string Series(int count, bool hub, bool distinctDimensions = true, string aggregation = "total", string value = "1") =>
        string.Join(",", Enumerable.Range(0, count).Select(index =>
        {
            var dimension = hub
                ? $$"""{"name":{"value":"EndpointName"},"value":"endpoint1"},{"name":{"value":"FailureReasonCategory"},"value":"reason{{(distinctDimensions ? index : 0)}}"}"""
                : $$"""{"name":{"value":"ResponseType"},"value":"response{{(distinctDimensions ? index : 0)}}"}""";
            return $$"""{"metadatavalues":[{{dimension}}],"data":[{"timeStamp":"2026-08-01T00:00:00Z","{{aggregation}}":{{value}}}]}""";
        }));

    private static HttpResponseMessage JsonResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode) { Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json") };

    private static RoutingDiagnosticsHttpMessageHandler CreateHandler(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> metricResponse,
        bool eventHub = false,
        int endpointCount = 1,
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage>? existenceResponse = null) =>
        new((request, token) =>
        {
            var path = request.RequestUri!.AbsolutePath;
            if (path.EndsWith("/metrics", StringComparison.Ordinal))
            {
                return metricResponse(request, token);
            }
            if (path.EndsWith("/providers/Microsoft.Devices", StringComparison.OrdinalIgnoreCase))
            {
                return JsonResponse($$"""{"id":"{{path}}","namespace":"Microsoft.Devices","registrationState":"Registered","resourceTypes":[{"resourceType":"IotHubs","locations":["westus"],"apiVersions":["2023-06-30"]}]}""");
            }
            if (path.EndsWith("/hub1", StringComparison.Ordinal))
            {
                var endpoints = string.Join(",", Enumerable.Range(1, endpointCount).Select(index =>
                    $$"""{"name":"endpoint{{index}}","subscriptionId":"{{Subscription}}","resourceGroup":"rg1","endpointUri":"{{(eventHub ? "sb://shared.servicebus.windows.net/" : "https://shared.blob.core.windows.net/")}}","entityPath":"eventhub{{index}}","containerName":"container{{index}}"}"""));
                return JsonResponse($$"""{"id":"{{HubId}}","name":"hub1","type":"Microsoft.Devices/IotHubs","location":"westus","properties":{"routing":{"endpoints":{"{{(eventHub ? "eventHubs" : "storageContainers")}}":[{{endpoints}}]} } } }""");
            }
            return existenceResponse?.Invoke(request, token) ?? JsonResponse("{}");
        });

    private static IoTHubRoutingService CreateService(HttpMessageHandler handler, IAzureService? azureService = null)
    {
        azureService ??= Substitute.For<IAzureService>();
        var configuration = Substitute.For<IAzureCloudConfiguration>();
        configuration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azureService.CloudConfiguration.Returns(configuration);
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1)));
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));
        azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        var subscription = Substitute.For<SubscriptionResource>();
        subscription.Data.Returns(ResourceManagerModelFactory.SubscriptionData(subscriptionId: Subscription));
        azureService.GetSubscription(Subscription, null, Arg.Any<CancellationToken>()).Returns(subscription);
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));
        return new IoTHubRoutingService(azureService, factory, Substitute.For<ILogger<IoTHubRoutingService>>());
    }
}
