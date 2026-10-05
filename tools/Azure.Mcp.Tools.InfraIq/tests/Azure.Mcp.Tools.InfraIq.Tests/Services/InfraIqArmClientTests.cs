// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.InfraIq.Commands;
using Azure.Mcp.Tools.InfraIq.Configuration;
using Azure.Mcp.Tools.InfraIq.Exceptions;
using Azure.Mcp.Tools.InfraIq.Models.Request;
using Azure.Mcp.Tools.InfraIq.Services;
using Azure.Mcp.Tools.InfraIq.Tests.TestSupport;
using Azure.ResourceManager;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.InfraIq.Tests.Services;

public class InfraIqArmClientTests
{
    private const string SubscriptionId = "12345678-1234-1234-1234-123456789012";
    private const string ApprovedOrigin = "https://eastus2euap.management.azure.com";

    private const string FullPayload = """
        {
          "options": [
            {
              "vmSize": "Standard_ND96isr_H100_v5",
              "recommendationRank": 1,
              "readiness": { "status": "SomeFuturePreviewStatus", "reason": "Ready", "newReadinessField": 7 },
              "topology": {
                "estimatedNodeCountPerReplica": 1,
                "estimatedTotalNodeCount": 2,
                "accelerator": { "model": "H100", "countPerVm": 8, "memoryGiBPerAccelerator": 80, "newAcceleratorField": true }
              },
              "cost": {
                "currencyCode": "USD",
                "vmHourly": 98.32,
                "deploymentHourly": 196.64,
                "source": "AzureSubscription",
                "priceAsOf": "2026-06-01T00:00:00Z",
                "error": { "code": "Throttled", "message": "pricing throttled" }
              },
              "quota": {
                "status": "Insufficient",
                "reason": "Need more quota",
                "error": { "code": "TimedOut", "message": "quota timed out" },
                "maxAvailableReplicaCount": 0,
                "increase": {
                  "limits": [ { "name": "standardNDSv5Family", "currentUsage": 0, "currentLimit": 0, "requiredLimit": 96, "extra": "x" } ],
                  "portalUrl": "https://portal.azure.com/quota"
                }
              },
              "placement": {
                "status": "Evaluated",
                "reason": "ok",
                "error": { "code": "DependencyError", "message": "placement failed" },
                "recommendedAvailabilityZones": ["1", "2"]
              },
              "azureMlRecommended": true,
              "benchmark": {
                "level": "TopologyMatched",
                "modelClass": "llama2-70b",
                "scenario": "Server",
                "submitterCount": 3,
                "submissionId": "sub-1",
                "benchmarkModel": "llama2-70b-99",
                "submitter": "Microsoft",
                "system": "ND96isr",
                "throughput": 1234.5,
                "throughputUnit": "tokens/s",
                "software": "vllm",
                "operatingSystem": "Ubuntu",
                "weightDataTypes": "fp16",
                "p99TtftMs": 450.5,
                "p99TpotMs": 33.3
              },
              "documentationUrl": "https://learn.microsoft.com/azure/virtual-machines/nd-h100-v5-series",
              "performanceEstimate": { "deploymentOutputTokensPerSecond": 900.25, "ttftMs": 120.5 },
              "newOptionField": { "nested": [1, 2, 3] }
            }
          ],
          "sizing": {
            "model": { "parameterCount": 70000000000, "weightPrecision": "FP16", "huggingFaceModelId": "meta-llama/Llama-3.1-70B-Instruct" },
            "workload": { "maxConcurrentRequestsPerReplica": 1, "contextLength": 4096 },
            "memory": { "modelWeightsGiB": 130.4, "kvCacheGiBPerRequest": 1.25, "kvCacheGiBTotal": 1.25, "modelAndKvCacheGiB": 131.65 },
            "basis": { "parameterCountSource": "HuggingFace", "modelWeightsCalculation": "ArtifactSize", "contextLengthSource": "Default" }
          },
          "newTopLevelField": "preview"
        }
        """;

    private const string MinimalPayload = """
        {
          "options": [],
          "sizing": {
            "model": { "parameterCount": 7000000000, "weightPrecision": "FP16" },
            "workload": { "maxConcurrentRequestsPerReplica": 1, "contextLength": 2048 },
            "memory": { "modelWeightsGiB": 13.0, "kvCacheGiBPerRequest": 0.5, "kvCacheGiBTotal": 0.5, "modelAndKvCacheGiB": 13.5 },
            "basis": { "parameterCountSource": "Request", "modelWeightsCalculation": "EstimatedFromParameterCount", "contextLengthSource": "Default" }
          }
        }
        """;

    private readonly IAzureService _azureService = Substitute.For<IAzureService>();
    private readonly FakeTokenCredential _credential = new();

    private static InfraIqRecommendVmSkuRequestBody Body => new()
    {
        Model = new() { HuggingFaceModelId = "meta-llama/Llama-3.1-70B-Instruct", WeightPrecision = "FP16" },
        Workload = new() { MaxConcurrentRequestsPerReplica = 2, TokenBudget = new() { PromptTokens = 100, MaxOutputTokens = 50 } },
        Deployment = new() { ReplicaCount = 2, ProcurementOption = "Spot" },
        Optimization = new() { RankingPreference = "Latency", MaxDeploymentCostPerHour = new() { Amount = 12.5, CurrencyCode = "USD" } },
        TargetVmSizes = ["Standard_ND96isr_H100_v5"],
        SubscriptionOptions = new() { IncludeQuota = true }
    };

    private InfraIqArmClient CreateClient(
        RecordingHttpMessageHandler handler,
        string? origin = ApprovedOrigin,
        ArmEnvironment? armEnvironment = null)
    {
        var cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment ?? ArmEnvironment.AzurePublicCloud);
        _azureService.CloudConfiguration.Returns(cloudConfiguration);
        _azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(_credential);
        _azureService.GetClient(Arg.Any<string?>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        return new InfraIqArmClient(_azureService, Microsoft.Extensions.Options.Options.Create(new InfraIqOptions { ArmIngressOrigin = origin }));
    }

    private static Task<Models.VmSku.VmSkuRecommendResult> CallAsync(InfraIqArmClient client, CancellationToken cancellationToken = default) =>
        client.RecommendVmSkuAsync(SubscriptionId, "eastus2euap", Body, tenant: null, cancellationToken);

    [Fact]
    public async Task RecommendVmSkuAsync_SendsExactAuthenticatedPostToApprovedOrigin()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        await CallAsync(client, TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            $"{ApprovedOrigin}/subscriptions/{SubscriptionId}/providers/Private.InfraIQ/locations/eastus2euap/recommendVmSku?api-version=2026-06-01-preview",
            request.Uri!.AbsoluteUri);
        Assert.Equal("Bearer", request.AuthorizationScheme);
        Assert.Equal("test-token", request.AuthorizationParameter);
        Assert.Equal(1, _credential.CallCount);
        _azureService.Received(1).GetClient(InfraIqArmClient.HttpClientName);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_EmitsExactlyOneApiVersion()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        await CallAsync(client, TestContext.Current.CancellationToken);

        var query = handler.First.Uri!.Query;
        Assert.Equal("?api-version=2026-06-01-preview", query);
        Assert.DoesNotContain("2026-09-01-preview", handler.First.Uri.AbsoluteUri);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_SetsClientRequestIdHeaders()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        await CallAsync(client, TestContext.Current.CancellationToken);

        Assert.True(Guid.TryParse(handler.First.Headers["x-ms-client-request-id"], out _));
        Assert.Equal("true", handler.First.Headers["x-ms-return-client-request-id"]);
        Assert.Equal("application/json", handler.First.Headers["Accept"]);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_SerializesOnlyApprovedBodyGroups()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        await CallAsync(client, TestContext.Current.CancellationToken);

        using var document = JsonDocument.Parse(handler.First.Body!);
        var names = document.RootElement.EnumerateObject().Select(property => property.Name).Order().ToArray();
        Assert.Equal(["deployment", "model", "optimization", "subscriptionOptions", "targetVmSizes", "workload"], names);
        Assert.Equal(12.5, document.RootElement.GetProperty("optimization").GetProperty("maxDeploymentCostPerHour").GetProperty("amount").GetDouble());
        Assert.False(document.RootElement.GetProperty("subscriptionOptions").GetProperty("includePricing").GetBoolean());
        Assert.DoesNotContain("subscriptionId", handler.First.Body);
        Assert.DoesNotContain("location", handler.First.Body);
        Assert.DoesNotContain("maxContextLength", handler.First.Body);
        Assert.DoesNotContain("null", handler.First.Body);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_ParsesFullPayloadIntoTypedGraphAndPreservesAdditions()
    {
        var handler = RecordingHttpMessageHandler.Json(FullPayload);
        handler = new RecordingHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(FullPayload, Encoding.UTF8, "application/json")
            };
            response.Headers.Add("x-ms-request-id", "req-123");
            response.Headers.Date = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
            return response;
        });
        var client = CreateClient(handler);

        var result = await CallAsync(client, TestContext.Current.CancellationToken);

        var option = Assert.Single(result.Response.Options);
        Assert.Equal("Standard_ND96isr_H100_v5", option.VmSize);
        Assert.Equal(1, option.RecommendationRank);
        Assert.Equal("SomeFuturePreviewStatus", option.Readiness.Status);
        Assert.Equal(7, option.Readiness.AdditionalProperties!["newReadinessField"].GetInt32());
        Assert.Equal(2, option.Topology.EstimatedTotalNodeCount);
        Assert.Equal("H100", option.Topology.Accelerator.Model);
        Assert.True(option.Topology.Accelerator.AdditionalProperties!["newAcceleratorField"].GetBoolean());
        Assert.Equal(98.32, option.Cost.VmHourly);
        Assert.Equal("AzureSubscription", option.Cost.Source);
        Assert.Equal(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero), option.Cost.PriceAsOf);
        Assert.Equal("Throttled", option.Cost.Error!.Code);
        Assert.Equal("Insufficient", option.Quota!.Status);
        Assert.Equal("TimedOut", option.Quota.Error!.Code);
        Assert.Equal(0, option.Quota.MaxAvailableReplicaCount);
        Assert.Equal(96, option.Quota.Increase!.Limits[0].RequiredLimit);
        Assert.Equal("x", option.Quota.Increase.Limits[0].AdditionalProperties!["extra"].GetString());
        Assert.Equal("Evaluated", option.Placement!.Status);
        Assert.Equal("DependencyError", option.Placement.Error!.Code);
        Assert.Equal(["1", "2"], option.Placement.RecommendedAvailabilityZones);
        Assert.True(option.AzureMlRecommended);
        Assert.Equal("TopologyMatched", option.Benchmark!.Level);
        Assert.Equal(450.5, option.Benchmark.P99TtftMs);
        Assert.Equal("https://learn.microsoft.com/azure/virtual-machines/nd-h100-v5-series", option.DocumentationUrl);
        Assert.Equal(120.5, option.PerformanceEstimate!.TtftMs);
        Assert.Equal(3, option.AdditionalProperties!["newOptionField"].GetProperty("nested").GetArrayLength());
        Assert.Equal(70000000000, result.Response.Sizing.Model.ParameterCount);
        Assert.Equal("meta-llama/Llama-3.1-70B-Instruct", result.Response.Sizing.Model.HuggingFaceModelId);
        Assert.Equal(4096, result.Response.Sizing.Workload.ContextLength);
        Assert.Equal(131.65, result.Response.Sizing.Memory.ModelAndKvCacheGiB);
        Assert.Equal("HuggingFace", result.Response.Sizing.Basis.ParameterCountSource);
        Assert.Equal("preview", result.Response.AdditionalProperties!["newTopLevelField"].GetString());

        Assert.Equal("req-123", result.Context.RequestId);
        Assert.Equal(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero), result.Context.Date);
        Assert.Null(result.Context.RetryAfter);
        Assert.True(Guid.TryParse(result.Context.ClientRequestId, out _));
    }

    [Fact]
    public async Task RecommendVmSkuAsync_RoundTripsAdditionsThroughSerialization()
    {
        var handler = RecordingHttpMessageHandler.Json(FullPayload);
        var client = CreateClient(handler);

        var result = await CallAsync(client, TestContext.Current.CancellationToken);
        var json = JsonSerializer.Serialize(result.Response, InfraIqJsonContext.Default.InfraIqRecommendVmSkuResponse);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("preview", document.RootElement.GetProperty("newTopLevelField").GetString());
        Assert.Equal(7, document.RootElement.GetProperty("options")[0].GetProperty("readiness").GetProperty("newReadinessField").GetInt32());
    }

    [Fact]
    public async Task RecommendVmSkuAsync_MinimalPayloadWithEmptyOptionsRemainsSuccess()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        var result = await CallAsync(client, TestContext.Current.CancellationToken);

        Assert.Empty(result.Response.Options);
        Assert.Equal(13.5, result.Response.Sizing.Memory.ModelAndKvCacheGiB);
        Assert.Null(result.Response.Sizing.Model.HuggingFaceModelId);
        Assert.Null(result.Response.AdditionalProperties);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_OptionWithOnlyStageErrorsRemainsSuccess()
    {
        const string payload = """
            {
              "options": [
                {
                  "vmSize": "Standard_NC24ads_A100_v4",
                  "recommendationRank": 2,
                  "readiness": { "status": "VerificationRequired", "reason": "Quota unknown" },
                  "topology": { "estimatedNodeCountPerReplica": 1, "estimatedTotalNodeCount": 1, "accelerator": { "model": "A100", "countPerVm": 1, "memoryGiBPerAccelerator": 80 } },
                  "cost": { "currencyCode": "USD", "vmHourly": 0, "deploymentHourly": 0, "source": "AzureRetail", "error": { "code": "DataUnavailable", "message": "no price" } },
                  "quota": { "status": "Unknown", "error": { "code": "Throttled", "message": "throttled" } },
                  "placement": { "status": "Failed", "error": { "code": "TimedOut", "message": "timed out" } }
                }
              ],
              "sizing": {
                "model": { "parameterCount": 1, "weightPrecision": "INT4" },
                "workload": { "maxConcurrentRequestsPerReplica": 1, "contextLength": 1 },
                "memory": { "modelWeightsGiB": 1, "kvCacheGiBPerRequest": 1, "kvCacheGiBTotal": 1, "modelAndKvCacheGiB": 2 },
                "basis": { "parameterCountSource": "Request", "modelWeightsCalculation": "ArtifactSize", "contextLengthSource": "Request" }
              }
            }
            """;
        var client = CreateClient(RecordingHttpMessageHandler.Json(payload));

        var result = await CallAsync(client, TestContext.Current.CancellationToken);

        var option = Assert.Single(result.Response.Options);
        Assert.Equal("DataUnavailable", option.Cost.Error!.Code);
        Assert.Equal("Throttled", option.Quota!.Error!.Code);
        Assert.Equal("TimedOut", option.Placement!.Error!.Code);
        Assert.Null(option.AzureMlRecommended);
        Assert.Null(option.DocumentationUrl);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_CostAmountsAreDoubles()
    {
        var client = CreateClient(RecordingHttpMessageHandler.Json(FullPayload));

        var result = await CallAsync(client, TestContext.Current.CancellationToken);

        Assert.IsType<double>(result.Response.Options[0].Cost.VmHourly);
        Assert.IsType<double>(result.Response.Options[0].Cost.DeploymentHourly);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://eastus2euap.management.azure.com")]
    [InlineData("https://evil.example.com")]
    [InlineData("https://eastus2euap.management.azure.com:8443")]
    [InlineData("https://eastus2euap.management.azure.com@evil.example.com")]
    [InlineData("https://eastus2euap.management.azure.com/path")]
    public async Task RecommendVmSkuAsync_RejectsUnapprovedOriginBeforeTokenOrTransport(string? origin)
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler, origin);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(0, _credential.CallCount);
        Assert.Equal(0, handler.CallCount);
        await _azureService.DidNotReceive().GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _azureService.DidNotReceive().GetClient(Arg.Any<string?>());
    }

    [Theory]
    [InlineData("..")]
    [InlineData("a/b")]
    [InlineData("a%2fb")]
    [InlineData("eastus2?x=1")]
    [InlineData("east us")]
    [InlineData("")]
    [InlineData("#frag")]
    [InlineData("a@evil.example.com")]
    public async Task RecommendVmSkuAsync_RejectsUnsafeLocationBeforeToken(string location)
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.RecommendVmSkuAsync(SubscriptionId, location, Body, null, TestContext.Current.CancellationToken));

        Assert.Equal(0, _credential.CallCount);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_AzurePublicCloud_RequestsPublicCloudArmScope()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler, armEnvironment: ArmEnvironment.AzurePublicCloud);

        await CallAsync(client, TestContext.Current.CancellationToken);

        Assert.Equal(1, _credential.CallCount);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_ChinaCloud_FailsClosedBeforeTokenOrTransport()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler, armEnvironment: ArmEnvironment.AzureChina);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(InfraIqArmClient.UnsupportedCloudMessage, exception.Message);
        Assert.Equal(0, _credential.CallCount);
        Assert.Equal(0, handler.CallCount);
        await _azureService.DidNotReceive().GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _azureService.DidNotReceive().GetClient(Arg.Any<string?>());
    }

    [Fact]
    public async Task RecommendVmSkuAsync_UsGovernmentCloud_FailsClosedBeforeTokenOrTransport()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler, armEnvironment: ArmEnvironment.AzureGovernment);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(InfraIqArmClient.UnsupportedCloudMessage, exception.Message);
        Assert.Equal(0, _credential.CallCount);
        Assert.Equal(0, handler.CallCount);
        await _azureService.DidNotReceive().GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _azureService.DidNotReceive().GetClient(Arg.Any<string?>());
    }

    [Fact]
    public async Task RecommendVmSkuAsync_UnknownCloud_FailsClosedBeforeTokenOrTransport()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var customEnvironment = new ArmEnvironment(new Uri("https://management.contoso.example"), "https://management.contoso.example");
        var client = CreateClient(handler, armEnvironment: customEnvironment);

        await Assert.ThrowsAsync<InvalidOperationException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(0, _credential.CallCount);
        Assert.Equal(0, handler.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EnsurePublicCloud_AcceptsOnlyPublicCloud(bool isPublic)
    {
        var environment = isPublic ? ArmEnvironment.AzurePublicCloud : ArmEnvironment.AzureGermany;

        if (isPublic)
        {
            InfraIqArmClient.EnsurePublicCloud(environment);
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => InfraIqArmClient.EnsurePublicCloud(environment));
        }
    }

    [Fact]
    public async Task RecommendVmSkuAsync_RejectsNonGuidSubscriptionBeforeToken()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.RecommendVmSkuAsync("../other", "eastus2", Body, null, TestContext.Current.CancellationToken));

        Assert.Equal(0, _credential.CallCount);
    }

    [Fact]
    public void CreateAndValidateRequestUri_BuildsExactEscapedDestination()
    {
        var uri = InfraIqArmClient.CreateAndValidateRequestUri(ApprovedOrigin + "/", SubscriptionId.ToUpperInvariant(), "eastus2euap");

        Assert.Equal(
            $"{ApprovedOrigin}/subscriptions/{SubscriptionId}/providers/Private.InfraIQ/locations/eastus2euap/recommendVmSku?api-version=2026-06-01-preview",
            uri.AbsoluteUri);
    }

    [Fact]
    public void CreateAndValidateRequestUri_RejectsUnsafeLocationAndSubscriptionSegments()
    {
        Assert.Throws<ArgumentException>(() =>
            InfraIqArmClient.CreateAndValidateRequestUri(ApprovedOrigin, SubscriptionId, "a-.."));
        Assert.Throws<ArgumentException>(() =>
            InfraIqArmClient.CreateAndValidateRequestUri(ApprovedOrigin, SubscriptionId, "-a"));
        Assert.Throws<ArgumentException>(() =>
            InfraIqArmClient.CreateAndValidateRequestUri(ApprovedOrigin, "not-a-guid", "eastus2"));
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(408)]
    [InlineData(409)]
    [InlineData(413)]
    [InlineData(415)]
    [InlineData(422)]
    [InlineData(429)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    [InlineData(418)]
    public async Task RecommendVmSkuAsync_MapsErrorStatusesToDeterministicSafeException(int status)
    {
        const string rawMessage = "raw backend message with secret-token and internal details";
        var errorBody = $$"""
            {
              "error": {
                "code": "InvalidArgument",
                "message": "{{rawMessage}}",
                "target": "model.huggingFaceModelId",
                "details": [ { "code": "Nested", "message": "nested detail text" } ],
                "additionalInfo": [ { "type": "Info", "info": { "leak": "additional info text" } } ]
              }
            }
            """;
        var handler = new RecordingHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage((HttpStatusCode)status)
            {
                Content = new StringContent(errorBody, Encoding.UTF8, "application/json")
            };
            response.Headers.Add("x-ms-request-id", "req-err");
            response.Headers.RetryAfter = new(TimeSpan.Zero);
            return response;
        });
        var client = CreateClient(handler);

        // Retries are Azure Core defaults; keep waits short by not asserting on call counts here.
        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(status, exception.Status);
        Assert.Equal(InfraIqErrorMessages.ForStatus(status), exception.Message);
        Assert.Equal("InvalidArgument", exception.Code);
        Assert.Equal("model.huggingFaceModelId", exception.Target);
        Assert.Equal("req-err", exception.RequestId);
        Assert.True(Guid.TryParse(exception.ClientRequestId, out _));
        Assert.DoesNotContain("secret-token", exception.ToString());
        Assert.DoesNotContain("nested detail", exception.Message);
        Assert.DoesNotContain("additional info", exception.Message);
        Assert.DoesNotContain(rawMessage, exception.Message);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_DefaultErrorMessageIsGeneric()
    {
        var client = CreateClient(RecordingHttpMessageHandler.Json("{}", HttpStatusCode.Gone));

        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal("The InfraIQ recommendation request failed.", exception.Message);
    }

    [Theory]
    [InlineData("120", "120")]
    [InlineData("0", "0")]
    [InlineData("Wed, 21 Oct 2026 07:28:00 GMT", "Wed, 21 Oct 2026 07:28:00 GMT")]
    [InlineData("  30  ", "30")]
    [InlineData("soon", null)]
    [InlineData("-5", null)]
    [InlineData("1.5", null)]
    [InlineData("12345678901", null)]
    [InlineData("Wed, 99 Oct 2026 07:28:00 GMT", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void TryReadRetryAfter_PreservesValidatedHeaderText(string? header, string? expected)
    {
        Assert.Equal(expected, InfraIqArmClient.TryReadRetryAfter(header));
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("Wed, 21 Oct 2015 07:28:00 GMT", "Wed, 21 Oct 2015 07:28:00 GMT")]
    [InlineData("garbage", null)]
    public async Task RecommendVmSkuAsync_ErrorResponsePreservesRetryAfterText(string header, string? expected)
    {
        // 413 is not retried by Azure Core, so the header is only inspected.
        var handler = new RecordingHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.RequestEntityTooLarge)
            {
                Content = new StringContent("{\"error\":{\"code\":\"TooLarge\"}}", Encoding.UTF8, "application/json")
            };
            response.Headers.TryAddWithoutValidation("Retry-After", header);
            return response;
        });
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(413, exception.Status);
        Assert.Equal(expected, exception.RetryAfter);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_UnsafeErrorMetadataIsDropped()
    {
        const string body = "{\"error\":{\"code\":\"bad code\\nwith newline\",\"target\":\"<script>\"}}";
        var client = CreateClient(RecordingHttpMessageHandler.Json(body, HttpStatusCode.BadRequest));

        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Null(exception.Code);
        Assert.Null(exception.Target);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_MalformedErrorJsonStillFailsExplicitly()
    {
        var client = CreateClient(RecordingHttpMessageHandler.Json("{ not json", HttpStatusCode.BadRequest));

        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(400, exception.Status);
        Assert.Null(exception.Code);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("null")]
    [InlineData("{\"options\":[]}")]
    [InlineData("")]
    public async Task RecommendVmSkuAsync_MalformedSuccessJsonFailsExplicitly(string body)
    {
        var client = CreateClient(RecordingHttpMessageHandler.Json(body));

        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(502, exception.Status);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_UnexpectedSuccessContentTypeFailsExplicitly()
    {
        var handler = new RecordingHttpMessageHandler(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(MinimalPayload, Encoding.UTF8, "text/html")
        });
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(502, exception.Status);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_RedirectIsNotFollowedAndSurfacedSafely()
    {
        var handler = new RecordingHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Found);
            response.Headers.Location = new Uri("https://evil.example.com/steal");
            return response;
        });
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(302, exception.Status);
        Assert.Equal(1, handler.CallCount);
        Assert.DoesNotContain(handler.Requests, request => request.Uri!.Host == "evil.example.com");
        Assert.DoesNotContain("evil.example.com", exception.ToString());
    }

    [Fact]
    public async Task RecommendVmSkuAsync_UsesAzureCoreDefaultRetryPolicy()
    {
        var handler = new RecordingHttpMessageHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            response.Headers.RetryAfter = new(TimeSpan.Zero);
            return response;
        });
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<InfraIqArmException>(() => CallAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(4, handler.CallCount);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_CancelledBeforeSend_DoesNotRequestTokenOrSend()
    {
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CallAsync(client, cts.Token));

        Assert.Equal(0, _credential.CallCount);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_CancelledDuringSend_Propagates()
    {
        using var cts = new CancellationTokenSource();
        var handler = new RecordingHttpMessageHandler(() =>
        {
            cts.Cancel();
            throw new OperationCanceledException(cts.Token);
        });
        var client = CreateClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CallAsync(client, cts.Token));
    }

    [Fact]
    public async Task RecommendVmSkuAsync_PassesTenantToCredentialResolution()
    {
        _azureService.ResolveTenantIdAsync("contoso", Arg.Any<CancellationToken>()).Returns("tenant-id");
        var handler = RecordingHttpMessageHandler.Json(MinimalPayload);
        var client = CreateClient(handler);

        await client.RecommendVmSkuAsync(SubscriptionId, "eastus2", Body, "contoso", TestContext.Current.CancellationToken);

        await _azureService.Received(1).GetTokenCredentialAsync("tenant-id", Arg.Any<CancellationToken>());
    }
}
