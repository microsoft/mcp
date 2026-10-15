// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:words slis

using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.ResilienceManagement.Models;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed class DrillsGaContractTests
{
    private const string Subscription = "11111111-1111-1111-1111-111111111111";
    private const string ServiceGroupId = "/providers/Microsoft.Management/serviceGroups/sg-ga";
    private const string ProviderPath = ServiceGroupId + "/providers/Microsoft.AzureResilienceManagement";
    private const string DrillPath = "/drills/drill-ga";
    private const string RunPath = DrillPath + "/drillRuns/run-ga";
    private const string TargetId = ProviderPath + DrillPath + "/drillResources/resource-ga";
    private const string HealthModelId = "/subscriptions/" + Subscription + "/resourceGroups/rg-ga/providers/Microsoft.CloudHealth/healthmodels/health-ga";
    private const string SliId = "/subscriptions/" + Subscription + "/resourceGroups/rg-ga/providers/Microsoft.Monitor/slis/sli-ga";
    private const string RecoveryResourceId = ProviderPath + "/recoveryPlans/plan-ga/recoveryResources/resource-ga";

    public static TheoryData<string, string, string> Operations => new()
    {
        { "list", "GET", "/drills" },
        { "get", "GET", DrillPath },
        { "create", "PUT", DrillPath },
        { "update", "PATCH", DrillPath },
        { "delete", "DELETE", DrillPath },
        { "start", "POST", DrillPath + "/start" },
        { "end", "POST", DrillPath + "/end" },
        { "check-resync-readiness", "POST", DrillPath + "/resyncReadinessCheck" },
        { "validate-for-execution", "POST", DrillPath + "/validateForExecution" },
        { "resource-list", "GET", DrillPath + "/drillResources" },
        { "resource-get", "GET", DrillPath + "/drillResources/resource-ga" },
        { "resource-add-or-update", "POST", DrillPath + "/addOrUpdateResources" },
        { "run-list", "GET", DrillPath + "/drillRuns" },
        { "run-get", "GET", RunPath },
        { "run-add-notes", "POST", RunPath + "/addNotes" },
        { "run-failover", "POST", RunPath + "/failOver" },
        { "run-resume", "POST", RunPath + "/resume" },
        { "run-mark-complete", "POST", RunPath + "/markAsComplete" },
        { "run-reprotect", "POST", RunPath + "/reprotect" },
        { "run-resource-list", "GET", RunPath + "/drillRunResources" },
        { "run-resource-get", "GET", RunPath + "/drillRunResources/resource-ga" }
    };

    [Theory]
    [InlineData(DrillKind.Zonal)]
    [InlineData(DrillKind.Regional)]
    public async Task CreateDrill_SendsGaConfiguration(DrillKind kind)
    {
        using var fixture = new Fixture();
        const string goal = """{"identity":{"type":"SystemAssigned"}}""";
        string health = $$"""{"identity":{"type":"SystemAssigned"},"healthModelId":"{{HealthModelId}}"}""";
        string sli = $$"""{"identity":{"type":"SystemAssigned"},"slis":[{"sliId":"{{SliId}}","type":"Availability"}]}""";

        await fixture.Service.CreateDrillAsync("sg-ga", "drill-ga", Subscription, "westus2", "rg-ga", kind,
            DrillRbacSetupMode.Manual, properties: DrillConfiguration.Create(kind, goal, health, sli),
            cancellationToken: TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(
            Assert.Single(fixture.Handler.Requests, request => request.Method == HttpMethod.Put).Body));
        JsonElement properties = document.RootElement.GetProperty("properties");
        Assert.Equal(kind.ToString(), properties.GetProperty("drillType").GetString());
        Assert.False(properties.GetProperty("goalAssignmentProperties").TryGetProperty("goalAssignmentId", out _));
        Assert.Equal(HealthModelId, properties.GetProperty("healthModelMonitoringProperties").GetProperty("healthModelId").GetString());
        Assert.Equal(SliId, properties.GetProperty("sliMonitoringProperties").GetProperty("slis")[0].GetProperty("sliId").GetString());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("null", null)]
    [InlineData(null, "null")]
    [InlineData("null", "null")]
    public async Task UpdateDrill_DistinguishesOmittedAndClearedMonitoring(string? healthModel, string? sli)
    {
        using var fixture = new Fixture();
        await fixture.Service.UpdateDrillAsync("sg-ga", "drill-ga", rbacSetupMode: DrillRbacSetupMode.Manual,
            properties: DrillConfiguration.Update(null, healthModel, sli),
            cancellationToken: TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(
            Assert.Single(fixture.Handler.Requests, request => request.Method == HttpMethod.Patch).Body));
        JsonElement properties = document.RootElement.GetProperty("properties");
        Assert.False(properties.TryGetProperty("goalAssignmentProperties", out _));
        foreach (string name in new[] { "healthModelMonitoringProperties", "sliMonitoringProperties" })
        {
            bool supplied = (name == "healthModelMonitoringProperties" ? healthModel : sli) is not null;
            Assert.Equal(supplied, properties.TryGetProperty(name, out JsonElement value));
            if (supplied)
            {
                Assert.Equal(JsonValueKind.Null, value.ValueKind);
            }
        }
    }

    [Fact]
    public async Task UpdateDrill_SendsPartialConfigurationAndReplacesSliSelection()
    {
        using var fixture = new Fixture();
        string health = $$"""{"healthModelId":"{{HealthModelId}}"}""";
        string sli = $$"""{"slis":[{"sliId":"{{SliId}}","type":"Latency"}]}""";
        await fixture.Service.UpdateDrillAsync("sg-ga", "drill-ga",
            properties: DrillConfiguration.Update(null, health, sli), cancellationToken: TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(
            Assert.Single(fixture.Handler.Requests, request => request.Method == HttpMethod.Patch).Body));
        JsonElement properties = document.RootElement.GetProperty("properties");
        Assert.False(properties.GetProperty("healthModelMonitoringProperties").TryGetProperty("identity", out _));
        Assert.Equal(HealthModelId, properties.GetProperty("healthModelMonitoringProperties").GetProperty("healthModelId").GetString());
        JsonElement sliProperties = properties.GetProperty("sliMonitoringProperties");
        Assert.False(sliProperties.TryGetProperty("identity", out _));
        Assert.Equal("Latency", Assert.Single(sliProperties.GetProperty("slis").EnumerateArray()).GetProperty("type").GetString());
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("Failover", true)]
    [InlineData("Reprotect", true)]
    [InlineData("Reprotect", false)]
    [InlineData("FailoverReverse", true)]
    [InlineData("ReprotectReverse", true)]
    public async Task ValidateDrill_SendsSupportedOperationAndLocationCombinations(string? operation, bool selectSource)
    {
        using var fixture = new Fixture();
        await fixture.Service.ValidateDrillForExecutionAsync("sg-ga", "drill-ga",
            sourceLocations: selectSource ? ["westus2-az1"] : null,
            operationName: operation is null ? (DrillRunTask?)null : new DrillRunTask(operation), cancellationToken: TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(Assert.Single(fixture.Handler.Requests).Body));
        JsonElement properties = document.RootElement.GetProperty("validateForExecutionProperties");
        Assert.Equal(selectSource, properties.TryGetProperty("sourceLocations", out JsonElement locations));
        if (selectSource)
        {
            Assert.Equal("westus2-az1", Assert.Single(locations.EnumerateArray()).GetString());
        }
        Assert.Equal(operation is not null, properties.TryGetProperty("operationName", out JsonElement name));
        if (operation is not null)
        {
            Assert.Equal(operation, name.GetString());
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Failover", null)]
    [InlineData("FailoverReverse", null)]
    [InlineData("ReprotectReverse", null)]
    [InlineData("Reprotect", "")]
    [InlineData("Reprotect", " ")]
    [InlineData("Failover", " ")]
    public async Task ValidateDrill_RejectsInvalidSourcesBeforeSending(string? operation, string? source)
    {
        using var fixture = new Fixture();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ValidateDrillForExecutionAsync(
            "sg-ga", "drill-ga",
            sourceLocations: source is null ? null : [source],
            operationName: operation is null ? (DrillRunTask?)null : new DrillRunTask(operation),
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(fixture.Handler.Requests);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Reprotect")]
    public async Task ValidateDrill_RejectsEmptySourceCollections(string? operation)
    {
        using var fixture = new Fixture();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ValidateDrillForExecutionAsync(
            "sg-ga", "drill-ga", [],
            operation is null ? (DrillRunTask?)null : new DrillRunTask(operation),
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Empty(fixture.Handler.Requests);
    }

    [Fact]
    public async Task FailoverDrillRun_OmitsBodyWhenInputsAreOmitted()
    {
        using var fixture = new Fixture();
        await fixture.Service.FailoverDrillRunAsync("sg-ga", "drill-ga", "run-ga", cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(string.IsNullOrEmpty(Assert.Single(fixture.Handler.Requests).Body));
    }

    [Fact]
    public async Task ReprotectDrillRun_SendsSelectedResourcesInGaEnvelope()
    {
        using var fixture = new Fixture();
        await fixture.Service.ReprotectDrillRunAsync("sg-ga", "drill-ga", "run-ga", [RecoveryResourceId],
            cancellationToken: TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(Assert.Single(fixture.Handler.Requests).Body));
        JsonElement selected = document.RootElement.GetProperty("reprotectProperties")
            .GetProperty("reprotectRequestProperties").GetProperty("selectedResourceIds");
        Assert.Equal(RecoveryResourceId, Assert.Single(selected.EnumerateArray()).GetString());
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task ExistingOperations_SendGaRequests(string operation, string method, string path)
    {
        using var fixture = new Fixture();

        await InvokeAsync(fixture.Service, operation, TestContext.Current.CancellationToken);

        CapturedRequest request = Assert.Single(fixture.Handler.Requests, request =>
            request.Method.Method == method && request.Uri.AbsolutePath == ProviderPath + path);
        Assert.All(fixture.Handler.Requests, request =>
        {
            Assert.Equal("management.azure.com", request.Uri.Host);
            Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(request.Uri.Query)["api-version"]);
        });
        if (method == "POST")
        {
            Assert.True(Guid.TryParse(request.OperationId, out _));
            Assert.Equal(request.OperationId, HttpUtility.ParseQueryString(request.Uri.Query)["operationId"]);
        }

        AssertRequestBody(operation, request.Body);
    }

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task ExistingOperations_PropagateGaErrors(string operation, string method, string path)
    {
        using var fixture = new Fixture
        {
            Handler =
            {
                StatusCode = HttpStatusCode.NotFound,
                ResponseBody = """{"error":{"code":"ResourceNotFound","message":"The test resource does not exist."}}"""
            }
        };

        RequestFailedException error = await Assert.ThrowsAsync<RequestFailedException>(() =>
            InvokeAsync(fixture.Service, operation, TestContext.Current.CancellationToken));

        Assert.Equal(404, error.Status);
        CapturedRequest request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(method, request.Method.Method);
        Assert.Equal(ProviderPath + path, request.Uri.AbsolutePath);
        Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(request.Uri.Query)["api-version"]);
    }

    [Theory]
    [InlineData("Zonal")]
    [InlineData("Regional")]
    public async Task GetDrill_PreservesGaProperties(string drillType)
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseBody = $$"""
            {
              "id":"{{ProviderPath + DrillPath}}","name":"drill-ga",
              "properties":{
                "drillType":"{{drillType}}","provisioningState":"NeedsAttention",
                "goalAssignmentProperties":{
                  "identity":{"type":"SystemAssigned"},
                  "goalAssignmentId":"{{ProviderPath}}/goalAssignments/goal-ga"
                },
                "healthModelMonitoringProperties":{
                  "identity":{"type":"SystemAssigned"},
                  "healthModelId":"{{HealthModelId}}"
                },
                "sliMonitoringProperties":{
                  "identity":{"type":"SystemAssigned"},
                  "slis":[{"sliId":"{{SliId}}","type":"Availability"}]
                }
              }
            }
            """;

        DrillInfo drill = await fixture.Service.GetDrillAsync("sg-ga", "drill-ga",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(drillType, drill.Properties.GetProperty("drillType").GetString());
        Assert.Equal("NeedsAttention", drill.Properties.GetProperty("provisioningState").GetString());
        Assert.Equal(ProviderPath + "/goalAssignments/goal-ga",
            drill.Properties.GetProperty("goalAssignmentProperties").GetProperty("goalAssignmentId").GetString());
        Assert.Equal(HealthModelId,
            drill.Properties.GetProperty("healthModelMonitoringProperties").GetProperty("healthModelId").GetString());
        JsonElement sli = Assert.Single(drill.Properties.GetProperty("sliMonitoringProperties").GetProperty("slis").EnumerateArray());
        Assert.Equal(SliId, sli.GetProperty("sliId").GetString());
        Assert.Equal("Availability", sli.GetProperty("type").GetString());
    }

    [Theory]
    [InlineData(false, "Disable")]
    [InlineData(true, "Enable")]
    public async Task FailoverDrillRun_PreservesAutomaticFailoverChoice(bool autoFailover, string expected)
    {
        using var fixture = new Fixture();

        await fixture.Service.FailoverDrillRunAsync("sg-ga", "drill-ga", "run-ga", ["westus2-az1"],
            autoFailover: autoFailover, cancellationToken: TestContext.Current.CancellationToken);

        using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(Assert.Single(fixture.Handler.Requests).Body));
        Assert.Equal(expected, document.RootElement.GetProperty("autoFailover").GetString());
        Assert.False(document.RootElement.GetProperty("failoverProperties").GetProperty("failoverRequestProperties")
            .TryGetProperty("selectedResourceIds", out _));
    }

    [Theory]
    [InlineData("resource-list", DrillPath + "/drillResources")]
    [InlineData("resource-get", DrillPath + "/drillResources/resource-ga")]
    public async Task DrillResourceReads_PropagateGaResponseValidationErrors(string operation, string path)
    {
        using var fixture = new Fixture
        {
            Handler =
            {
                StatusCode = HttpStatusCode.PreconditionFailed,
                ResponseBody = """
                    {"error":{"code":"HttpResponsePayloadAPISpecValidationFailed",
                    "message":"Missing required property: drillType.",
                    "details":[{"code":"ObjectMissingRequiredProperty","message":"Missing required property: drillType."}]}}
                    """
            }
        };

        var error = await Assert.ThrowsAsync<RequestFailedException>(() =>
            InvokeAsync(fixture.Service, operation, TestContext.Current.CancellationToken));

        Assert.Equal(412, error.Status);
        Assert.Equal("HttpResponsePayloadAPISpecValidationFailed", error.ErrorCode);
        Assert.Contains("drillType", error.Message);
        var request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(ProviderPath + path, request.Uri.AbsolutePath);
        Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(request.Uri.Query)["api-version"]);
    }

    [Theory]
    [InlineData("Zonal")]
    [InlineData("Regional")]
    public async Task GetDrillResource_PreservesGaDiscriminatorAndProperties(string drillType)
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseBody = $$"""
            {"id":"{{ProviderPath + DrillPath}}/drillResources/resource-ga","name":"resource-ga","properties":{
              "drillType":"{{drillType}}","provisioningState":"NeedsAttention",
              "activePhysicalZones":["westus2-az1"],"additionalProperty":{"enabled":true}
            } }
            """;

        DrillResourceInfo resource = await fixture.Service.GetDrillResourceAsync("sg-ga", "drill-ga", "resource-ga",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(drillType, resource.Properties.GetProperty("drillType").GetString());
        Assert.Equal("NeedsAttention", resource.Properties.GetProperty("provisioningState").GetString());
        Assert.True(resource.Properties.GetProperty("additionalProperty").GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public async Task GetDrillRun_PreservesReport()
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseBody = $$"""
            {
              "id":"{{ProviderPath + RunPath}}","name":"run-ga",
              "properties":{"report":{"status":"Completed","additionalProperty":true} }
            }
            """;

        JsonElement run = await fixture.Service.GetDrillRunAsync("sg-ga", "drill-ga", "run-ga",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(run.GetProperty("properties").GetProperty("report").GetProperty("additionalProperty").GetBoolean());
    }

    [Fact]
    public async Task GetDrillRunResource_PreservesNeedsAttention()
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseBody = $$"""
            {
              "id":"{{ProviderPath + RunPath}}/drillRunResources/resource-ga","name":"resource-ga",
              "properties":{"provisioningState":"NeedsAttention"}
            }
            """;

        JsonElement resource = await fixture.Service.GetDrillRunResourceAsync("sg-ga", "drill-ga", "run-ga", "resource-ga",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("NeedsAttention", resource.GetProperty("properties").GetProperty("provisioningState").GetString());
    }

    private static Task InvokeAsync(ResilienceManagementService service, string operation, CancellationToken token) =>
        operation switch
        {
            "list" => service.ListDrillsAsync("sg-ga", cancellationToken: token),
            "get" => service.GetDrillAsync("sg-ga", "drill-ga", cancellationToken: token),
            "create" => service.CreateDrillAsync("sg-ga", "drill-ga", Subscription, "westus2", "rg-ga",
                DrillKind.Zonal, DrillRbacSetupMode.Manual, cancellationToken: token),
            "update" => service.UpdateDrillAsync("sg-ga", "drill-ga", rbacSetupMode: DrillRbacSetupMode.Manual, cancellationToken: token),
            "delete" => service.DeleteDrillAsync("sg-ga", "drill-ga", cancellationToken: token),
            "start" => service.StartDrillAsync("sg-ga", "drill-ga", "Failover", cancellationToken: token),
            "end" => service.EndDrillAsync("sg-ga", "drill-ga", "Success", "GA test completed", cancellationToken: token),
            "check-resync-readiness" => service.CheckDrillResyncReadinessAsync("sg-ga", "drill-ga", cancellationToken: token),
            "validate-for-execution" => service.ValidateDrillForExecutionAsync("sg-ga", "drill-ga", ["westus2-az1"], cancellationToken: token),
            "resource-list" => service.ListDrillResourcesAsync("sg-ga", "drill-ga", cancellationToken: token),
            "resource-get" => service.GetDrillResourceAsync("sg-ga", "drill-ga", "resource-ga", cancellationToken: token),
            "resource-add-or-update" => service.AddOrUpdateDrillResourcesAsync("sg-ga", "drill-ga",
                DrillAddOrUpdateResourcesCommand.CreateContent(new DrillAddOrUpdateResourcesOptions
                {
                    ServiceGroup = "sg-ga",
                    Drill = "drill-ga",
                    FaultDurationMinutes = 5,
                    IncludeResources = $$"""[{"id":"{{TargetId}}"}]""",
                    ForceInclusionAndUpdate = "Enable"
                }), cancellationToken: token),
            "run-list" => service.ListDrillRunsAsync("sg-ga", "drill-ga", cancellationToken: token),
            "run-get" => service.GetDrillRunAsync("sg-ga", "drill-ga", "run-ga", cancellationToken: token),
            "run-add-notes" => service.AddDrillRunNotesAsync("sg-ga", "drill-ga", "run-ga", "GA notes", cancellationToken: token),
            "run-failover" => service.FailoverDrillRunAsync("sg-ga", "drill-ga", "run-ga", ["westus2-az1"],
                [RecoveryResourceId], cancellationToken: token),
            "run-resume" => service.ResumeDrillRunAsync("sg-ga", "drill-ga", "run-ga", cancellationToken: token),
            "run-mark-complete" => service.MarkDrillRunCompleteAsync("sg-ga", "drill-ga", "run-ga", "FaultInjection", cancellationToken: token),
            "run-reprotect" => service.ReprotectDrillRunAsync("sg-ga", "drill-ga", "run-ga", cancellationToken: token),
            "run-resource-list" => service.ListDrillRunResourcesAsync("sg-ga", "drill-ga", "run-ga", cancellationToken: token),
            "run-resource-get" => service.GetDrillRunResourceAsync("sg-ga", "drill-ga", "run-ga", "resource-ga", cancellationToken: token),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown test operation.")
        };

    private static void AssertRequestBody(string operation, string? body)
    {
        string? expected = operation switch
        {
            "update" => """{"properties":{"rbacSetupMode":"Manual"}}""",
            "start" => """{"mode":"Failover"}""",
            "end" => """{"attestation":"Success","attestationNotes":"GA test completed"}""",
            "validate-for-execution" => """{"validateForExecutionProperties":{"sourceLocations":["westus2-az1"]}}""",
            "resource-add-or-update" => $$"""
                {"faultDurationInMin":5,"resourceLists":{"includeResources":[{"id":"{{TargetId}}"}],
                "updateResources":[],"excludeResources":[]},"forceInclusionAndUpdate":"Enable"}
                """,
            "run-add-notes" => """{"notes":"GA notes"}""",
            "run-failover" => $$"""
                {"autoFailover":"Disable","failoverProperties":{"failoverDirection":"FromSpecificLocations",
                "failoverRequestProperties":{"sourceLocations":["westus2-az1"],"selectedResourceIds":["{{RecoveryResourceId}}"]} } }
                """,
            "run-mark-complete" => """{"drillRunStage":"FaultInjection"}""",
            _ => null
        };
        if (operation == "create")
        {
            using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(body));
            JsonElement properties = document.RootElement.GetProperty("properties");
            Assert.Equal("Zonal", properties.GetProperty("drillType").GetString());
            Assert.Equal("Manual", properties.GetProperty("rbacSetupMode").GetString());
            Assert.Equal(Subscription, properties.GetProperty("drillAssetProperties").GetProperty("subscription").GetString());
            Assert.Equal("westus2", properties.GetProperty("drillAssetProperties").GetProperty("region").GetString());
            Assert.Equal("rg-ga", properties.GetProperty("drillAssetProperties").GetProperty("resourceGroup").GetString());
            Assert.False(properties.TryGetProperty("managedOnBehalfOfConfiguration", out _));
            return;
        }
        if (expected is null)
        {
            Assert.True(string.IsNullOrEmpty(body), $"Unexpected request body for {operation}: {body}");
            return;
        }
        using JsonDocument expectedDocument = JsonDocument.Parse(expected);
        using JsonDocument actualDocument = JsonDocument.Parse(Assert.IsType<string>(body));
        Assert.True(JsonElement.DeepEquals(expectedDocument.RootElement, actualDocument.RootElement),
            $"Unexpected request body for {operation}: {body}");
    }

    private sealed class Fixture : IDisposable
    {
        public CapturingHandler Handler { get; } = new();
        private readonly HttpClient _client;
        public ResilienceManagementService Service { get; }

        public Fixture()
        {
            _client = new HttpClient(Handler);
            IAzureService azureService = AzureServiceTestHelpers.CreateAzureService();
            AzureServiceTestHelpers.ConfigureArmClientOptions(azureService, _client, ArmEnvironment.AzurePublicCloud);
            TokenCredential credential = Substitute.For<TokenCredential>();
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                .Returns(new AccessToken("unit-test-token", DateTimeOffset.MaxValue));
            azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
            azureService.IsSubscriptionId(Subscription).Returns(true);
            Service = new ResilienceManagementService(azureService);
        }

        public void Dispose() => _client.Dispose();
    }

    private sealed record CapturedRequest(HttpMethod Method, Uri Uri, string? Body, string? OperationId);

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];
        public HttpStatusCode? StatusCode { get; set; }
        public string? ResponseBody { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri uri = Assert.IsType<Uri>(request.RequestUri);
            string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            string? operationId = request.Headers.TryGetValues("operation-id", out IEnumerable<string>? values) ? values.Single() : null;
            Requests.Add(new(request.Method, uri, body, operationId));

            bool isList = uri.AbsolutePath.EndsWith("/drills", StringComparison.Ordinal) ||
                uri.AbsolutePath.EndsWith("/drillResources", StringComparison.Ordinal) ||
                uri.AbsolutePath.EndsWith("/drillRuns", StringComparison.Ordinal) ||
                uri.AbsolutePath.EndsWith("/drillRunResources", StringComparison.Ordinal);
            string resourceId = uri.AbsolutePath + (isList ? "/example" : string.Empty);
            string resource = $$"""
                {"id":"{{resourceId}}","name":"example","properties":{"drillType":"Zonal","provisioningState":"Succeeded"} }
                """;
            string responseBody = ResponseBody ?? (isList ? $$"""{"value":[{{resource}}]}""" : resource);
            HttpStatusCode status = StatusCode ?? (request.Method == HttpMethod.Delete ? HttpStatusCode.NoContent : HttpStatusCode.OK);
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
