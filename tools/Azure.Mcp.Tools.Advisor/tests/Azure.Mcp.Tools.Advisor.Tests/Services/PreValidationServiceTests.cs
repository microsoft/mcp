// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public sealed class PreValidationServiceTests
{
    private const string Subscription = "00000000-0000-0000-0000-000000000001";
    private const string Tenant = "00000000-0000-0000-0000-000000000002";
    private const string TypeId = "48eda464-1485-4dcf-a674-d0905df5054a";
    private const string ResourceId = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.Compute/disks/disk";
    private const string LocksPath = $"/subscriptions/{Subscription}/providers/Microsoft.Authorization/locks";
    private static readonly DateTimeOffset Now = new(2026, 10, 30, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ValidateAsync_MismatchedRecommendationGuardRequiresReviewWithoutAzureReads()
    {
        var azure = Substitute.For<IAzureService>();
        var service = CreateService(azure);
        var candidate = Candidate(typeId: Guid.Empty.ToString());

        var result = await service.ValidateAsync(Guid.Empty.ToString(), [candidate], null, false, [], 30,
            TestContext.Current.CancellationToken);

        Assert.Equal("needsReview", result.Verdict);
        Assert.Single(result.NeedsReview);
        Assert.Empty(result.Safe);
        Assert.Empty(azure.ReceivedCalls());
    }

    [Fact]
    public async Task ValidateAsync_EmptyCandidatesDoNotClaimSafetyOrReadAzure()
    {
        var azure = Substitute.For<IAzureService>();
        var result = await CreateService(azure).ValidateAsync(TypeId, [], null, false, [], 30,
            TestContext.Current.CancellationToken);

        Assert.Equal("needsReview", result.Verdict);
        Assert.Empty(azure.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ValidateAsync_ProcessesEveryBatchAndPreservesUnknownEvidence(bool truncated)
    {
        var azure = Substitute.For<IAzureService>();
        var typeId = Guid.Empty.ToString();
        var candidates = Enumerable.Range(0, 126).Select(index =>
            Candidate(typeId, $"{ResourceId}-{index}")).ToArray();
        var result = await CreateService(azure).ValidateAsync(typeId,
            candidates.Concat(candidates.Take(1)).ToArray(), null, truncated, ["missing"], 30,
            TestContext.Current.CancellationToken);

        Assert.Equal(126, result.NeedsReview.Count);
        Assert.Equal(candidates.Select(static candidate => candidate.Properties.ResourceMetadata!.ResourceId),
            result.NeedsReview.Select(static resource => resource.ResourceId));
        Assert.Empty(result.Safe);
        Assert.Equal("needsReview", result.Verdict);
        Assert.Equal(truncated, result.AreResultsTruncated);
        Assert.Equal(126, result.CheckedResourceCount);
        Assert.Equal(truncated ? (int?)null : 0, result.RemainingResourceCount);
        Assert.Equal(["missing"], result.UnmatchedResources);
        Assert.Equal(126, result.Summary.Sum(static bucket => bucket.Count));
        Assert.Empty(azure.ReceivedCalls());
    }

    [Fact]
    public async Task ValidateAsync_CancellationPropagatesBeforeAzureReads()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        var azure = Substitute.For<IAzureService>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateService(azure).ValidateAsync(TypeId, [Candidate()], null, false, [], 30, cancellation.Token));

        Assert.Empty(azure.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ValidateAsync_BoundsCombinedArmArgAndMonitorReadsAndPreservesCheckOrder(bool pauseOnlyArmGroups)
    {
        const string planTypeId = "39a8510f-5bbf-4304-9bcd-4106c996473b";
        const string planId = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.Web/serverfarms/plan";
        var firstWave = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var enabled = false;
        var active = 0;
        async Task PauseRead(CancellationToken token)
        {
            var count = Interlocked.Increment(ref active);
            try
            {
                Assert.InRange(count, 1, 4);
                if (count == 4)
                {
                    firstWave.TrySetResult();
                }
                await release.Task.WaitAsync(token);
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        }
        using var handler = new PreValidationHttpMessageHandler(
            (request, body) => Respond(request, body, "complete", planId),
            (request, token) => enabled && (!pauseOnlyArmGroups ||
                request.RequestUri!.AbsolutePath.EndsWith("/resources", StringComparison.Ordinal))
                    ? PauseRead(token) : Task.CompletedTask);
        var azure = await CreateAzureServiceAsync(handler);
        var metrics = Substitute.For<IMonitorMetricsService>();
        metrics.QueryMetricsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(async call =>
            {
                if (!pauseOnlyArmGroups)
                {
                    await PauseRead(call.Arg<CancellationToken>());
                }
                return [];
            });
        enabled = true;
        var candidates = Enumerable.Range(0, 9).Select(index => Candidate(planTypeId,
            planId.Replace("/resourceGroups/rg/", $"/resourceGroups/rg{index}/", StringComparison.Ordinal))).ToArray();
        var validation = CreateService(azure, metrics).ValidateAsync(planTypeId, candidates,
            Tenant, false, [], 30, TestContext.Current.CancellationToken);
        try
        {
            await firstWave.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            Assert.Equal(4, Volatile.Read(ref active));
        }
        finally
        {
            release.TrySetResult();
            await validation;
        }

        var result = await validation;
        Assert.Equal(9, result.Safe.Count + result.NeedsReview.Count + result.Blocked.Count);
        var expectedOrder = ResourceInsightsCatalog.All["microsoft.web/serverfarms"].Checks.Select(static check => check.Id);
        Assert.All(result.Safe.Concat(result.NeedsReview).Concat(result.Blocked), resource =>
            Assert.Equal(expectedOrder, resource.Checks.Where(static check => check.Id != "deletion-status").Select(static check => check.Id)));
        Assert.Equal(0, Volatile.Read(ref active));
    }

    [Theory]
    [InlineData("complete", "safe")]
    [InlineData("recent", "needsReview")]
    [InlineData("unknown-age", "needsReview")]
    [InlineData("missing-field", "needsReview")]
    [InlineData("truncated", "needsReview")]
    [InlineData("unexpected-key", "needsReview")]
    [InlineData("read-denied", "needsReview")]
    public async Task ValidateAsync_RunsResourceSpecificChecksAndFailsClosed(string scenario, string verdict)
    {
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
            Respond(request, body, scenario));
        var azure = await CreateAzureServiceAsync(handler);

        var result = await CreateService(azure).ValidateAsync(TypeId, [Candidate()], Tenant, false, [], 30,
            TestContext.Current.CancellationToken);

        Assert.Equal(verdict, result.Verdict);
        var resource = Assert.Single(result.Safe.Concat(result.NeedsReview));
        Assert.Equal(ResourceId, resource.ResourceId);
        Assert.Equal(ResourceInsightsCatalog.All["microsoft.compute/disks"].Checks.Count, resource.Checks.Count);
        Assert.Contains(resource.Checks, static check => check.Outcome == "manual" && check.Guidance != null);
        Assert.Equal(150, resource.YearlyPotentialSavings);
        Assert.Null(resource.Cost);
        Assert.Null(resource.ResourceDeletedOn);
        Assert.Null(resource.Reason);
        Assert.Null(resource.CanPostponeDismiss);
        if (scenario == "recent")
        {
            Assert.Contains(resource.Checks, static check => check.Id == "unused-age" && check.Outcome == "warning");
        }
    }

    [Theory]
    [InlineData("confirmed", true)]
    [InlineData("oldest-observable", true)]
    [InlineData("missing", false)]
    [InlineData("malformed", false)]
    [InlineData("wrong-type", false)]
    [InlineData("out-of-window", false)]
    [InlineData("future", false)]
    [InlineData("duplicate", false)]
    [InlineData("unexpected-key", false)]
    [InlineData("truncated", false)]
    [InlineData("read-denied", false)]
    [InlineData("paged", true)]
    public async Task ValidateAsync_ConfirmsDeletionOnlyFromCompleteRecentHistory(string scenario, bool confirmed)
    {
        var deletionQueries = new List<string>();
        var deletedOn = scenario == "oldest-observable" ? Now.AddDays(-14) : Now.AddDays(-2);
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
        {
            if (request.Method != HttpMethod.Post)
            {
                return Respond(request, body, "complete");
            }
            using var document = JsonDocument.Parse(body!);
            var query = document.RootElement.GetProperty("query").GetString()!;
            if (!query.Contains("summarize deletedOn", StringComparison.Ordinal))
            {
                return Respond(request, body, "scope-blocked");
            }
            deletionQueries.Add(query);
            if (scenario == "read-denied")
            {
                return new(HttpStatusCode.Forbidden);
            }
            if (scenario == "paged" && deletionQueries.Count == 1)
            {
                return Json(new Dictionary<string, object>
                {
                    ["totalRecords"] = 1,
                    ["count"] = 0,
                    ["resultTruncated"] = "true",
                    ["$skipToken"] = "next",
                    ["data"] = Array.Empty<object>(),
                });
            }
            object timestamp = scenario switch
            {
                "malformed" => "not-a-date",
                "wrong-type" => 123,
                "out-of-window" => Now.AddDays(-14).AddSeconds(-1).ToString("O"),
                "future" => Now.AddSeconds(1).ToString("O"),
                _ => deletedOn.ToString("O"),
            };
            object[] rows = scenario == "missing" ? [] :
                [new { key = scenario == "unexpected-key" ? "/unselected-resource" : ResourceId.ToUpperInvariant(), deletedOn = timestamp }];
            if (scenario == "duplicate")
            {
                rows = [rows[0], rows[0]];
            }
            return Json(new
            {
                totalRecords = rows.Length,
                count = rows.Length,
                resultTruncated = scenario == "truncated" ? "true" : "false",
                data = rows
            });
        });
        var azure = await CreateAzureServiceAsync(handler);

        var result = await CreateService(azure).ValidateAsync(TypeId, [Candidate()], Tenant, false, [], 30,
            TestContext.Current.CancellationToken);

        var resource = Assert.Single(result.Blocked);
        Assert.Empty(result.Safe);
        Assert.Empty(result.NeedsReview);
        Assert.Contains(resource.Checks, static check => check.Id == "recheck-detection" && check.Outcome == "failed");
        var probe = Assert.Single(resource.Checks, static check => check.Id == "deletion-status");
        Assert.All(deletionQueries, static query =>
        {
            Assert.Contains("resourcechanges", query);
            Assert.Contains("ago(14d)", query);
            Assert.Contains("changeType) =~ 'Delete'", query);
            Assert.Contains(ResourceId.ToLowerInvariant(), query);
            Assert.Contains("max(changeTime)", query);
        });
        Assert.Equal(scenario == "paged" ? 2 : 1, deletionQueries.Count);
        Assert.Equal(confirmed ? deletedOn : (DateTimeOffset?)null, resource.ResourceDeletedOn);
        Assert.Equal(confirmed ? 1 : 0, Assert.Single(result.Summary, static bucket => bucket.Verdict == "blocked").DeletedCount);
        if (confirmed)
        {
            Assert.NotNull(resource.Reason);
            Assert.Contains("already deleted", resource.Reason);
            Assert.Contains("No action is needed", resource.Reason);
            Assert.Contains("within 72 hours", resource.Reason);
            Assert.DoesNotContain("48 hours", resource.Reason);
            Assert.False(resource.CanPostponeDismiss);
            Assert.Empty(resource.ManualConfirmations);
            Assert.Equal("info", probe.Outcome);
            Assert.Single(probe.Data!);
            Assert.Equal(150, resource.YearlyPotentialSavings);
        }
        else
        {
            Assert.Null(resource.Reason);
            Assert.Null(resource.CanPostponeDismiss);
            Assert.Equal(scenario == "missing" ? "info" : "unverified", probe.Outcome);
            Assert.Contains("original block", probe.Evidence, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData("complete", "safe")]
    [InlineData("recent", "needsReview")]
    [InlineData("locked", "blocked")]
    public async Task ValidateAsync_DoesNotProbeDeletionWithoutFailedScopeChecks(string scenario, string verdict)
    {
        var deletionQueries = new List<string>();
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                using var document = JsonDocument.Parse(body!);
                var query = document.RootElement.GetProperty("query").GetString()!;
                if (query.Contains("summarize deletedOn", StringComparison.Ordinal))
                {
                    deletionQueries.Add(query);
                }
            }
            if (scenario == "locked" && request.RequestUri!.AbsolutePath == LocksPath)
            {
                return Json(new
                {
                    value = new[] { new { id = ResourceId + "/providers/Microsoft.Authorization/locks/protected",
                    properties = new { level = "CanNotDelete" } } }
                });
            }
            return Respond(request, body, scenario == "recent" ? "recent" : "complete");
        });
        var azure = await CreateAzureServiceAsync(handler);

        var result = await CreateService(azure).ValidateAsync(TypeId, [Candidate()], Tenant, false, [], 30,
            TestContext.Current.CancellationToken);

        Assert.Equal(verdict, result.Verdict);
        var resource = Assert.Single(result.Safe.Concat(result.NeedsReview).Concat(result.Blocked));
        Assert.Null(resource.ResourceDeletedOn);
        Assert.DoesNotContain(resource.Checks, static check => check.Id == "deletion-status");
        Assert.Empty(deletionQueries);
    }

    [Fact]
    public async Task ValidateAsync_MixedBatchProbesOnlyScopeFailuresAndPreservesOtherBuckets()
    {
        const string deletedId = ResourceId + "-deleted";
        const string lockedId = ResourceId + "-locked";
        var ids = new[] { ResourceId, deletedId, lockedId };
        var deletionQueries = new List<string>();
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
        {
            if (request.RequestUri!.AbsolutePath == LocksPath)
            {
                return Json(new
                {
                    value = new[] { new { id = lockedId + "/providers/Microsoft.Authorization/locks/protected",
                    properties = new { level = "CanNotDelete" } } }
                });
            }
            if (request.Method == HttpMethod.Get && request.RequestUri.AbsolutePath.EndsWith("/resources", StringComparison.Ordinal))
            {
                return Json(new
                {
                    value = ids.Where(id => id != deletedId)
                    .Select(id => new { id, changedTime = Now.AddDays(-60) }).ToArray()
                });
            }
            if (request.Method == HttpMethod.Post)
            {
                using var document = JsonDocument.Parse(body!);
                var query = document.RootElement.GetProperty("query").GetString()!;
                object[] rows;
                if (query.Contains("summarize deletedOn", StringComparison.Ordinal))
                {
                    deletionQueries.Add(query);
                    rows = [new { key = deletedId, deletedOn = Now.AddDays(-2) }];
                }
                else if (query.Contains("managedBy ==", StringComparison.Ordinal))
                {
                    rows = ids.Where(id => id != deletedId).Select(id => (object)new { key = id }).ToArray();
                }
                else if (query.Contains("lastActivityAt", StringComparison.Ordinal))
                {
                    rows = ids.Select(id => (object)new { key = id, lastActivityAt = Now.AddDays(-60) }).ToArray();
                }
                else if (query.Contains("project key = tolower(id), diskState", StringComparison.Ordinal))
                {
                    rows = ids.Select(id => (object)new { key = id, diskState = "Unattached" }).ToArray();
                }
                else
                {
                    return Respond(request, body, "complete");
                }
                return Json(new { totalRecords = rows.Length, count = rows.Length, resultTruncated = "false", data = rows });
            }
            return Respond(request, body, "complete");
        });
        var azure = await CreateAzureServiceAsync(handler);

        var result = await CreateService(azure).ValidateAsync(TypeId, ids.Select(id => Candidate(resourceId: id)).ToArray(),
            Tenant, false, [], 30, TestContext.Current.CancellationToken);

        Assert.Equal(ResourceId, Assert.Single(result.Safe).ResourceId);
        Assert.Empty(result.NeedsReview);
        Assert.Equal(2, result.Blocked.Count);
        var deleted = Assert.Single(result.Blocked, static resource => resource.ResourceDeletedOn is not null);
        Assert.Equal(deletedId, deleted.ResourceId);
        Assert.False(deleted.CanPostponeDismiss);
        var locked = Assert.Single(result.Blocked, static resource => resource.ResourceDeletedOn is null);
        Assert.Equal(lockedId, locked.ResourceId);
        Assert.Null(locked.CanPostponeDismiss);
        Assert.DoesNotContain(locked.Checks, static check => check.Id == "deletion-status");
        var query = Assert.Single(deletionQueries);
        Assert.Contains($"'{deletedId.ToLowerInvariant()}'", query);
        Assert.DoesNotContain($"'{ResourceId.ToLowerInvariant()}'", query);
        Assert.DoesNotContain($"'{lockedId.ToLowerInvariant()}'", query);
        Assert.Equal(1, Assert.Single(result.Summary, static bucket => bucket.Verdict == "blocked").DeletedCount);
    }

    [Fact]
    public async Task ValidateAsync_DeletionProbeCancellationPropagates()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
        {
            if (request.Method == HttpMethod.Post)
            {
                using var document = JsonDocument.Parse(body!);
                if (document.RootElement.GetProperty("query").GetString()!.Contains("summarize deletedOn", StringComparison.Ordinal))
                {
                    cancellation.Cancel();
                    throw new OperationCanceledException(cancellation.Token);
                }
            }
            return Respond(request, body, "scope-blocked");
        });
        var azure = await CreateAzureServiceAsync(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateService(azure).ValidateAsync(TypeId, [Candidate()], Tenant, false, [], 30, cancellation.Token));
    }

    [Theory]
    [InlineData(1, 30, "needsReview", 0)]
    [InlineData(29, 30, "needsReview", 0)]
    [InlineData(30, 30, "safe", 0)]
    [InlineData(30, 30, "safe", 56)]
    [InlineData(45, 30, "safe", 0)]
    [InlineData(44, 45, "needsReview", 0)]
    [InlineData(45, 45, "safe", 0)]
    [InlineData(-1, 30, "needsReview", 0)]
    [InlineData(365, 365, "needsReview", 0)]
    public async Task ValidateAsync_AppServiceAgeUsesDeclaredMetricsAndRequiresCompleteHistory(
        int daysUnused, int minimumUnusedDays, string verdict, int seconds)
    {
        const string planId = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.Web/serverfarms/plan";
        const string planTypeId = "39a8510f-5bbf-4304-9bcd-4106c996473b";
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
            Respond(request, body, "complete", planId));
        var azure = await CreateAzureServiceAsync(handler);
        var metrics = Substitute.For<IMonitorMetricsService>();
        var lookbackDays = Math.Min(minimumUnusedDays + 1, 93);
        var start = Now.AddDays(-lookbackDays);
        var values = new double[lookbackDays + 1];
        if (daysUnused >= 0 && daysUnused < lookbackDays)
        {
            values[lookbackDays - daysUnused - 1] = 100;
        }
        var results = daysUnused < 0 ? new List<MetricResult>() : new[] { "BytesReceived", "BytesSent" }
            .Select(name => new MetricResult
            {
                Name = name,
                TimeSeries = [new()
                {
                    Start = start.UtcDateTime, End = Now.UtcDateTime, Interval = "P1D", TotalBuckets = values,
                }],
            }).ToList();
        metrics.QueryMetricsAsync(Subscription, "rg", "Microsoft.Web/serverfarms", planId, "Microsoft.Web/serverfarms",
            Arg.Any<IEnumerable<string>>(), Arg.Any<string?>(), Arg.Any<string?>(), "P1D", "Total",
            tenant: Tenant, cancellationToken: Arg.Any<CancellationToken>()).Returns(results);

        var result = await CreateService(azure, metrics, Now.AddSeconds(seconds)).ValidateAsync(planTypeId, [Candidate(planTypeId, planId)],
            Tenant, false, [], minimumUnusedDays, TestContext.Current.CancellationToken);

        Assert.Equal(verdict, result.Verdict);
        var resource = Assert.Single(result.Safe.Concat(result.NeedsReview));
        Assert.Equal(ResourceInsightsCatalog.All["microsoft.web/serverfarms"].Checks.Count, resource.Checks.Count);
        var age = Assert.Single(resource.Checks, static check => check.Id == "unused-age");
        Assert.Equal(verdict == "safe" ? "passed" : daysUnused < 0 || minimumUnusedDays > 93 ? "unverified" : "warning", age.Outcome);
        Assert.Contains(resource.Checks, static check => check.Outcome == "manual" && check.Guidance != null);
        await metrics.Received(1).QueryMetricsAsync(Subscription, "rg", "Microsoft.Web/serverfarms", planId, "Microsoft.Web/serverfarms",
            Arg.Is<IEnumerable<string>>(names => names.SequenceEqual(new[] { "BytesReceived", "BytesSent" })),
            start.ToString("O"), Now.ToString("O"), "P1D", "Total",
            tenant: Tenant, cancellationToken: Arg.Is<CancellationToken>(token => token.CanBeCanceled));
    }

    [Theory]
    [InlineData("complete", "safe")]
    [InlineData("recent", "needsReview")]
    [InlineData("gap", "needsReview")]
    public async Task ValidateAsync_RealMonitorServicePreservesDailyCoverageSemantics(string scenario, string verdict)
    {
        const string planId = $"/subscriptions/{Subscription}/resourceGroups/rg/providers/Microsoft.Web/serverfarms/plan";
        const string planTypeId = "39a8510f-5bbf-4304-9bcd-4106c996473b";
        var metricReads = 0;
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
        {
            if (!request.RequestUri!.AbsolutePath.EndsWith("/providers/microsoft.insights/metrics", StringComparison.OrdinalIgnoreCase))
            {
                return Respond(request, body, "complete", planId);
            }
            Interlocked.Increment(ref metricReads);
            var query = Uri.UnescapeDataString(request.RequestUri.Query);
            Assert.Contains("metricnames=BytesReceived,BytesSent", query, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("interval=P1D", query, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("aggregation=Total", query, StringComparison.OrdinalIgnoreCase);
            return Json(new
            {
                value = new[] { "BytesReceived", "BytesSent" }.Select(name => new
                {
                    name = new { value = name },
                    unit = "Bytes",
                    timeseries = new[]
                    {
                        new
                        {
                            data = Enumerable.Range(0, 32).Select(index => new
                            {
                                timeStamp = Now.AddDays(index - 31),
                                total = scenario == "gap" && index == 5 ? (double?)null :
                                    scenario == "recent" && index == 30 ? 1d : 0d,
                            }).ToArray(),
                        },
                    },
                }).ToArray(),
            });
        });
        var azure = await CreateAzureServiceAsync(handler);
        azure.When(service => service.ConfigureArmClientOptions(Arg.Any<ArmClientOptions>()))
            .Do(call => call.Arg<ArmClientOptions>().Transport = new HttpClientTransport(
                new HttpClient(handler, disposeHandler: false)));
        var resolver = Substitute.For<IResourceResolverService>();
        resolver.ResolveResourceIdAsync(Subscription, "rg", "Microsoft.Web/serverfarms", planId, Tenant, Arg.Any<CancellationToken>())
            .Returns(new ResourceIdentifier(planId));

        var result = await CreateService(azure, new MonitorMetricsService(resolver, azure)).ValidateAsync(planTypeId,
            [Candidate(planTypeId, planId)], Tenant, false, [], 30, TestContext.Current.CancellationToken);

        Assert.Equal(verdict, result.Verdict);
        Assert.Equal(1, metricReads);
        var age = Assert.Single(Assert.Single(result.Safe.Concat(result.NeedsReview)).Checks,
            static check => check.Id == "unused-age");
        Assert.Equal(scenario == "complete" ? "passed" : scenario == "gap" ? "unverified" : "warning", age.Outcome);
    }

    [Theory]
    [InlineData("subscription", false)]
    [InlineData("resource-group", false)]
    [InlineData("resource", false)]
    [InlineData("resource-group", true)]
    public async Task ValidateAsync_InheritedLocksStayBlockedWhenLaterPagesFail(string scope, bool failSecondPage)
    {
        var lockScope = scope switch
        {
            "subscription" => $"/subscriptions/{Subscription}",
            "resource-group" => $"/subscriptions/{Subscription}/resourceGroups/rg",
            _ => ResourceId,
        };
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
        {
            if (request.RequestUri!.AbsolutePath == LocksPath)
            {
                if (request.RequestUri.Query.Contains("skiptoken", StringComparison.Ordinal))
                {
                    return new(HttpStatusCode.Forbidden);
                }
                return Json(new
                {
                    value = new[] { new { id = lockScope + "/providers/Microsoft.Authorization/locks/protected",
                        properties = new { level = "CanNotDelete" } } },
                    nextLink = failSecondPage
                        ? "https://management.azure.com" + LocksPath + "?api-version=2016-09-01&$skiptoken=next"
                        : null,
                });
            }
            return Respond(request, body, "complete");
        });
        var azure = await CreateAzureServiceAsync(handler);

        var result = await CreateService(azure).ValidateAsync(TypeId, [Candidate()], Tenant, false, [], 30,
            TestContext.Current.CancellationToken);

        Assert.Equal("blocked", result.Verdict);
        var resource = Assert.Single(result.Blocked);
        Assert.Contains(resource.Checks, static check => check.Id == "resource-locks" && check.Outcome == "failed");
    }

    [Fact]
    public async Task ValidateAsync_DoesNotForwardTokensToAnUnrelatedPaginationHost()
    {
        var requests = new List<Uri>();
        using var handler = new PreValidationHttpMessageHandler((request, body) =>
        {
            requests.Add(request.RequestUri!);
            return request.RequestUri!.AbsolutePath == LocksPath
                ? Json(new { value = Array.Empty<object>(), nextLink = "https://unrelated.example/collect" })
                : Respond(request, body, "complete");
        });
        var azure = await CreateAzureServiceAsync(handler);

        await Assert.ThrowsAsync<System.Security.SecurityException>(() =>
            CreateService(azure).ValidateAsync(TypeId, [Candidate()], Tenant, false, [], 30,
                TestContext.Current.CancellationToken));

        Assert.DoesNotContain(requests, static uri => uri.Host == "unrelated.example");
    }

    private static PreValidationService CreateService(
        IAzureService azure, IMonitorMetricsService? metrics = null, DateTimeOffset? now = null)
    {
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now ?? Now);
        return new(azure, metrics ?? Substitute.For<IMonitorMetricsService>(), NullLogger<PreValidationService>.Instance, clock);
    }

    private static Models.Recommendation Candidate(string typeId = TypeId, string resourceId = ResourceId)
    {
        using var savings = JsonDocument.Parse("""{"retail":{"yearlyPotentialSavings":150},"resourceDeletedOn":"2026-10-01T00:00:00Z"}""");
        return new(new RecommendationProperties("Cost", ImpactedValue: "disk", RecommendationStatus: "New",
            RecommendationTypeId: typeId, ResourceMetadata: new(resourceId), Savings: savings.RootElement.Clone()),
            Name: "recommendation");
    }

    private static async Task<IAzureService> CreateAzureServiceAsync(HttpMessageHandler handler)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1))));
        var options = new ArmClientOptions { Transport = new HttpClientTransport(new HttpClient(handler, disposeHandler: false)) };
        options.Retry.MaxRetries = 0;
        var arm = new ArmClient(credential, defaultSubscriptionId: null, options);
        var subscription = (await arm.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(Subscription))
            .GetAsync(TestContext.Current.CancellationToken)).Value;
        var tenants = new List<TenantResource>();
        await foreach (var tenant in arm.GetTenants().GetAllAsync(TestContext.Current.CancellationToken))
        {
            tenants.Add(tenant);
        }
        var azure = Substitute.For<IAzureService>();
        var cloud = Substitute.For<IAzureCloudConfiguration>();
        cloud.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        azure.CloudConfiguration.Returns(cloud);
        azure.GetSubscription(Subscription, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(subscription);
        azure.GetTenants(Arg.Any<CancellationToken>()).Returns(tenants);
        azure.ResolveTenantIdAsync(Tenant, Arg.Any<CancellationToken>()).Returns(Tenant);
        azure.GetTokenCredentialAsync(Tenant, Arg.Any<CancellationToken>()).Returns(credential);
        azure.GetClient().Returns(_ => new HttpClient(handler, disposeHandler: false));
        return azure;
    }

    private static HttpResponseMessage Respond(HttpRequestMessage request, string? body, string scenario, string resourceId = ResourceId)
    {
        var path = request.RequestUri!.AbsolutePath;
        if (request.Method == HttpMethod.Post)
        {
            using var document = JsonDocument.Parse(body!);
            var query = document.RootElement.GetProperty("query").GetString()!;
            if (scenario == "read-denied")
            {
                return new(HttpStatusCode.Forbidden) { Content = new StringContent("""{"error":{"code":"Forbidden","message":"Denied"}}""") };
            }
            object[] rows = query.Contains("lastActivityAt", StringComparison.Ordinal)
                ? [new { key = resourceId.ToLowerInvariant(),
                    lastActivityAt = scenario == "unknown-age" ? (DateTimeOffset?)null : Now.AddDays(scenario == "recent" ? -5 : -400) }]
                : query.Contains("noChangesIn14Days", StringComparison.Ordinal)
                    ? [new { key = resourceId.ToLowerInvariant(), noChangesIn14Days = true }]
                    : query.Contains("managedBy ==", StringComparison.Ordinal) || query.Contains("where toint(properties.numberOfSites)", StringComparison.Ordinal)
                        ? scenario == "scope-blocked" ? [] : [new { key = resourceId.ToLowerInvariant() }]
                        : query.Contains("project key = tolower(id), diskState", StringComparison.Ordinal)
                            ? scenario == "missing-field"
                                ? [new { key = resourceId.ToLowerInvariant() }]
                                : [new { key = resourceId.ToLowerInvariant(), diskState = "Unattached" }]
                            : [];
            if (scenario == "unexpected-key")
            {
                rows = [new { key = "/unselected-resource" }];
            }
            return Json(new
            {
                totalRecords = rows.Length,
                count = rows.Length,
                resultTruncated = scenario == "truncated" ? "true" : "false",
                data = rows
            });
        }
        if (path == "/tenants")
        {
            return Json(new { value = new[] { new { id = $"/tenants/{Tenant}", tenantId = Tenant } } });
        }
        if (path == $"/subscriptions/{Subscription}")
        {
            return Json(new { id = path, subscriptionId = Subscription, tenantId = Tenant, displayName = "Test", state = "Enabled" });
        }
        if (path == LocksPath)
        {
            return Json(new { value = Array.Empty<object>() });
        }
        if (path.EndsWith("/resources", StringComparison.Ordinal))
        {
            return Json(new { value = new[] { new { id = resourceId, changedTime = Now.AddDays(-400) } } });
        }
        throw new InvalidOperationException($"Unexpected mocked ARM request: {request.Method} {path}");
    }

    private static HttpResponseMessage Json(object payload) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
    };
}
