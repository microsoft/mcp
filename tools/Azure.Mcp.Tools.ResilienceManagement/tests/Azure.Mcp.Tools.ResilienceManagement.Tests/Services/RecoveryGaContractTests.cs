// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed class RecoveryGaContractTests
{
    private const string Subscription = "11111111-1111-1111-1111-111111111111";
    private const string ServiceGroup = "sg-ga";
    private const string RecoveryPlan = "plan-ga";
    private const string RecoveryResource = "22222222-2222-2222-2222-222222222222";
    private const string RecoveryJob = "33333333-3333-3333-3333-333333333333";
    private const string RecoveryJobTarget = "target-ga";
    private const string ProviderPath = "/providers/Microsoft.Management/serviceGroups/" + ServiceGroup +
        "/providers/Microsoft.AzureResilienceManagement";
    private const string PlanPath = ProviderPath + "/recoveryPlans/" + RecoveryPlan;
    private const string ResourcePath = PlanPath + "/recoveryResources/" + RecoveryResource;
    private const string JobPath = PlanPath + "/recoveryJobs/" + RecoveryJob;
    private const string TargetPath = JobPath + "/recoveryJobResources/" + RecoveryJobTarget;
    private const string RecoveryResourceId = ResourcePath;

    public static TheoryData<string> Operations => new()
    {
        "ListRecoveryPlans",
        "GetRecoveryPlan",
        "CreateOrUpdateRecoveryPlan",
        "DeleteRecoveryPlan",
        "UpdateRecoveryResources",
        "FailoverRecoveryPlan",
        "FinalizeRecoveryPlan",
        "ReprotectRecoveryPlan",
        "ValidateRecoveryPlanForFailover",
        "ValidateRecoveryPlanForReprotect",
        "ValidateRecoveryPlanForOperation",
        "CheckRecoveryPlanReadiness",
        "ListRecoveryResources",
        "GetRecoveryResource",
        "ListRecoveryJobs",
        "GetRecoveryJob",
        "RetryRecoveryJob",
        "ResumeRecoveryJob",
        "ListRecoveryJobTargets",
        "GetRecoveryJobTarget"
    };

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task AllRecoverySdkOperations_SendExpectedGaRequests(string operation)
    {
        using var fixture = new Fixture();

        await InvokeAsync(fixture.Service, operation, TestContext.Current.CancellationToken);

        ExpectedRequest[] expected = GetExpectedRequests(operation);
        Assert.Equal(expected.Length, fixture.Handler.Requests.Count);
        for (int index = 0; index < expected.Length; index++)
        {
            CapturedRequest actual = fixture.Handler.Requests[index];
            Assert.Equal(index, actual.Sequence);
            Assert.Equal(expected[index].Method, actual.Method);
            Assert.Equal("management.azure.com", actual.Uri.Host);
            Assert.Equal(expected[index].Path, actual.Uri.AbsolutePath);

            var query = HttpUtility.ParseQueryString(actual.Uri.Query);
            Assert.Equal("api-version", Assert.Single(query.AllKeys));
            Assert.Equal("2026-10-01", query["api-version"]);

            if (actual.Method == HttpMethod.Post)
            {
                Assert.True(Guid.TryParse(actual.OperationId, out _));
            }
            else
            {
                Assert.Null(actual.OperationId);
            }
        }
    }

    private static Task InvokeAsync(ResilienceManagementService service, string operation, CancellationToken token) =>
        operation switch
        {
            "ListRecoveryPlans" => service.ListRecoveryPlansAsync(ServiceGroup, cancellationToken: token),
            "GetRecoveryPlan" => service.GetRecoveryPlanAsync(ServiceGroup, RecoveryPlan, cancellationToken: token),
            "CreateOrUpdateRecoveryPlan" => service.CreateRecoveryPlanAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryPlanKind.Zonal,
                "GA contract plan",
                RecoveryPlanIdentityKind.SystemAssigned,
                cancellationToken: token),
            "DeleteRecoveryPlan" => service.DeleteRecoveryPlanAsync(ServiceGroup, RecoveryPlan, cancellationToken: token),
            "UpdateRecoveryResources" => service.UpdateRecoveryPlanResourcesAsync(
                ServiceGroup,
                RecoveryPlan,
                CreateUpdateContent(),
                cancellationToken: token),
            "FailoverRecoveryPlan" => service.FailoverRecoveryPlanAsync(
                ServiceGroup,
                RecoveryPlan,
                ["westus2"],
                [RecoveryResourceId],
                cancellationToken: token),
            "FinalizeRecoveryPlan" => service.FinalizeRecoveryPlanAsync(ServiceGroup, RecoveryPlan, cancellationToken: token),
            "ReprotectRecoveryPlan" => service.ReprotectRecoveryPlanAsync(
                ServiceGroup,
                RecoveryPlan,
                [RecoveryResourceId],
                cancellationToken: token),
            "ValidateRecoveryPlanForFailover" => service.ValidateRecoveryPlanForFailoverAsync(
                ServiceGroup,
                RecoveryPlan,
                ["westus2"],
                [RecoveryResourceId],
                cancellationToken: token),
            "ValidateRecoveryPlanForReprotect" => service.ValidateRecoveryPlanForReprotectAsync(
                ServiceGroup,
                RecoveryPlan,
                [RecoveryResourceId],
                cancellationToken: token),
            "ValidateRecoveryPlanForOperation" => service.ValidateRecoveryPlanForOperationAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryOperationName.Failover,
                cancellationToken: token),
            "CheckRecoveryPlanReadiness" => service.CheckRecoveryPlanReadinessAsync(
                ServiceGroup,
                RecoveryPlan,
                cancellationToken: token),
            "ListRecoveryResources" => service.ListRecoveryResourcesAsync(ServiceGroup, RecoveryPlan, cancellationToken: token),
            "GetRecoveryResource" => service.GetRecoveryResourceAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryResource,
                cancellationToken: token),
            "ListRecoveryJobs" => service.ListRecoveryJobsAsync(ServiceGroup, RecoveryPlan, cancellationToken: token),
            "GetRecoveryJob" => service.GetRecoveryJobAsync(ServiceGroup, RecoveryPlan, RecoveryJob, cancellationToken: token),
            "RetryRecoveryJob" => service.RetryRecoveryJobAsync(ServiceGroup, RecoveryPlan, RecoveryJob, cancellationToken: token),
            "ResumeRecoveryJob" => service.ResumeRecoveryJobAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryJob,
                "Continue the GA contract job",
                cancellationToken: token),
            "ListRecoveryJobTargets" => service.ListRecoveryJobResourcesAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryJob,
                cancellationToken: token),
            "GetRecoveryJobTarget" => service.GetRecoveryJobResourceAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryJob,
                RecoveryJobTarget,
                cancellationToken: token),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown recovery operation.")
        };

    private static UpdateRecoveryResourcesContent CreateUpdateContent() =>
        RecoveryPlanUpdateResourcesCommand.CreateContent(new RecoveryPlanUpdateResourcesOptions
        {
            ServiceGroup = ServiceGroup,
            RecoveryPlan = RecoveryPlan,
            ResourcesToUpdate = $$"""
                [{
                  "id":"{{RecoveryResourceId}}",
                  "properties":{
                    "recoveryResourceUniqueId":"{{RecoveryResource}}",
                    "inclusionState":"Excluded"
                  }
                }]
                """
        });

    private static ExpectedRequest[] GetExpectedRequests(string operation) =>
        operation switch
        {
            "ListRecoveryPlans" => [Get(ProviderPath + "/recoveryPlans")],
            "GetRecoveryPlan" => [Get(PlanPath)],
            "CreateOrUpdateRecoveryPlan" => [Get(PlanPath), Put(PlanPath)],
            "DeleteRecoveryPlan" => [Get(PlanPath), Delete(PlanPath)],
            "UpdateRecoveryResources" => [Get(PlanPath), Get(ResourcePath), Post(PlanPath + "/updateResources")],
            "FailoverRecoveryPlan" => [Get(PlanPath), Post(PlanPath + "/failover")],
            "FinalizeRecoveryPlan" => [Get(PlanPath), Post(PlanPath + "/finalize")],
            "ReprotectRecoveryPlan" => [Get(PlanPath), Post(PlanPath + "/reprotect")],
            "ValidateRecoveryPlanForFailover" => [Get(PlanPath), Post(PlanPath + "/validateForFailover")],
            "ValidateRecoveryPlanForReprotect" => [Get(PlanPath), Post(PlanPath + "/validateForReprotect")],
            "ValidateRecoveryPlanForOperation" => [Get(PlanPath), Post(PlanPath + "/validateForOperation")],
            "CheckRecoveryPlanReadiness" =>
            [
                Get(PlanPath),
                Get(PlanPath + "/recoveryJobs"),
                Post(PlanPath + "/checkReadiness"),
                Get(JobPath),
                Get(JobPath + "/recoveryJobResources")
            ],
            "ListRecoveryResources" => [Get(PlanPath), Get(PlanPath + "/recoveryResources")],
            "GetRecoveryResource" => [Get(PlanPath), Get(ResourcePath)],
            "ListRecoveryJobs" => [Get(PlanPath + "/recoveryJobs")],
            "GetRecoveryJob" => [Get(JobPath)],
            "RetryRecoveryJob" => [Get(JobPath), Post(JobPath + "/retry")],
            "ResumeRecoveryJob" => [Get(JobPath), Post(JobPath + "/resume")],
            "ListRecoveryJobTargets" => [Get(JobPath), Get(JobPath + "/recoveryJobResources")],
            "GetRecoveryJobTarget" => [Get(TargetPath)],
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "Unknown recovery operation.")
        };

    private static ExpectedRequest Get(string path) => new(HttpMethod.Get, path);
    private static ExpectedRequest Put(string path) => new(HttpMethod.Put, path);
    private static ExpectedRequest Post(string path) => new(HttpMethod.Post, path);
    private static ExpectedRequest Delete(string path) => new(HttpMethod.Delete, path);

    private sealed class Fixture : IDisposable
    {
        private readonly HttpClient _client;
        private readonly TestArmService _armService;

        public Fixture()
        {
            Handler = new CapturingHandler();
            _client = new HttpClient(Handler);
            IAzureService azureService = AzureServiceTestHelpers.CreateAzureService();
            AzureServiceTestHelpers.ConfigureArmClientOptions(azureService, _client, ArmEnvironment.AzurePublicCloud);
            TokenCredential credential = Substitute.For<TokenCredential>();
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                .Returns(new AccessToken("unit-test-token", DateTimeOffset.MaxValue));
            azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
            azureService.IsSubscriptionId(Subscription).Returns(true);
            _armService = new TestArmService(azureService);
            Service = new ResilienceManagementService(azureService);
        }

        public CapturingHandler Handler { get; }
        public ResilienceManagementService Service { get; }
        public Task<ArmClient> CreateArmClientAsync(CancellationToken cancellationToken) =>
            _armService.GetArmClientAsync(cancellationToken);

        public void Dispose() => _client.Dispose();
    }

    private sealed class TestArmService(IAzureService azureService) : BaseAzureService(azureService)
    {
        public Task<ArmClient> GetArmClientAsync(CancellationToken cancellationToken) =>
            CreateArmClientAsync(cancellationToken: cancellationToken);
    }

    private sealed record ExpectedRequest(HttpMethod Method, string Path);

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri Uri,
        string? Body,
        string? OperationId,
        int Sequence);

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];
        public Func<HttpMethod, Uri, string?, HttpResponseMessage?>? ResponseFactory { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Uri uri = Assert.IsType<Uri>(request.RequestUri);
            string? body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            string? operationId = request.Headers.TryGetValues("operation-id", out IEnumerable<string>? values)
                ? values.Single()
                : null;
            Requests.Add(new(request.Method, uri, body, operationId, Requests.Count));

            HttpResponseMessage? overrideResponse = ResponseFactory?.Invoke(request.Method, uri, body);
            if (overrideResponse is not null)
            {
                return overrideResponse;
            }

            HttpStatusCode status = request.Method == HttpMethod.Delete
                ? HttpStatusCode.NoContent
                : HttpStatusCode.OK;
            string responseBody = GetResponseBody(uri.AbsolutePath, request.Method);
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }

        private static string GetResponseBody(string path, HttpMethod method)
        {
            if (path.EndsWith("/recoveryPlans", StringComparison.Ordinal))
            {
                return $$"""{"value":[{{PlanResourceJson}}]}""";
            }

            if (path.EndsWith("/recoveryResources", StringComparison.Ordinal))
            {
                return $$"""{"value":[{{RecoveryResourceJson}}]}""";
            }

            if (path.EndsWith("/recoveryJobs", StringComparison.Ordinal))
            {
                return """{"value":[]}""";
            }

            if (path.EndsWith("/recoveryJobResources", StringComparison.Ordinal))
            {
                return $$"""{"value":[{{RecoveryJobTargetJson}}]}""";
            }

            if (path.Contains("/recoveryJobResources/", StringComparison.Ordinal))
            {
                return RecoveryJobTargetJson;
            }

            if (path.EndsWith("/updateResources", StringComparison.Ordinal))
            {
                return """{"failedResources":[]}""";
            }

            if (path.EndsWith("/validateForFailover", StringComparison.Ordinal) ||
                path.EndsWith("/validateForReprotect", StringComparison.Ordinal))
            {
                return """{"properties":{"recoveryResourceQualifications":[]}}""";
            }

            if (path.EndsWith("/validateForOperation", StringComparison.Ordinal))
            {
                return """{"properties":{"error":{"code":"None"}}}""";
            }

            if (path.EndsWith("/checkReadiness", StringComparison.Ordinal) ||
                path.EndsWith("/failover", StringComparison.Ordinal) ||
                path.EndsWith("/reprotect", StringComparison.Ordinal))
            {
                return $$"""{"jobId":"{{RecoveryJob}}"}""";
            }

            if (path.EndsWith("/finalize", StringComparison.Ordinal) ||
                path.EndsWith("/retry", StringComparison.Ordinal) ||
                path.EndsWith("/resume", StringComparison.Ordinal))
            {
                return "{}";
            }

            if (path.Contains("/recoveryJobs/", StringComparison.Ordinal))
            {
                return RecoveryJobJson;
            }

            if (path.Contains("/recoveryResources/", StringComparison.Ordinal))
            {
                return RecoveryResourceJson;
            }

            if (path.EndsWith("/recoveryPlans/" + RecoveryPlan, StringComparison.Ordinal) ||
                method == HttpMethod.Put)
            {
                return PlanResourceJson;
            }

            throw new InvalidOperationException($"No fake response is configured for {method} {path}.");
        }

        private static string PlanResourceJson => $$"""
            {
              "id":"{{PlanPath}}",
              "name":"{{RecoveryPlan}}",
              "type":"Microsoft.AzureResilienceManagement/recoveryPlans",
              "identity":{"type":"SystemAssigned"},
              "properties":{
                "provisioningState":"Succeeded",
                "planType":"Zonal",
                "planState":"Ready",
                "planDescription":"GA contract plan",
                "recoveryGroupsSetting":{
                  "defaultGroup":{
                    "properties":{
                      "groupUniqueId":"44444444-4444-4444-4444-444444444444",
                      "orderId":1,
                      "description":"Default group"
                    }
                  },
                  "additionalGroups":[]
                }
              }
            }
            """;

        private static string RecoveryResourceJson => $$"""
            {
              "id":"{{ResourcePath}}",
              "name":"{{RecoveryResource}}",
              "type":"Microsoft.AzureResilienceManagement/recoveryPlans/recoveryResources",
              "properties":{
                "recoveryResourceUniqueId":"{{RecoveryResource}}",
                "inclusionState":"Included"
              }
            }
            """;

        private static string RecoveryJobJson => $$"""
            {
              "id":"{{JobPath}}",
              "name":"{{RecoveryJob}}",
              "type":"Microsoft.AzureResilienceManagement/recoveryPlans/recoveryJobs",
              "properties":{
                "status":"Completed",
                "operation":"CheckRecoveryPlanReadiness",
                "provisioningState":"Succeeded"
              }
            }
            """;

        private static string RecoveryJobTargetJson => $$"""
            {
              "id":"{{TargetPath}}",
              "name":"{{RecoveryJobTarget}}",
              "type":"Microsoft.AzureResilienceManagement/recoveryPlans/recoveryJobs/recoveryJobResources",
              "properties":{
                "status":"Completed",
                "resourceId":"{{RecoveryResourceId}}",
                "provisioningState":"Succeeded"
              }
            }
            """;
    }

    [Fact]
    public async Task Failover_SerializesGaPayloadNamesAndTypes()
    {
        using var fixture = new Fixture();

        await fixture.Service.FailoverRecoveryPlanAsync(
            ServiceGroup,
            RecoveryPlan,
            ["westus2", "eastus2"],
            [RecoveryResourceId],
            "Approve",
            cancellationToken: TestContext.Current.CancellationToken);

        JsonElement root = ParseActionBody(fixture.Handler, PlanPath + "/failover");
        AssertFailoverPayload(root);
    }

    [Fact]
    public async Task ValidateForFailover_SerializesGaPayloadNamesAndTypes()
    {
        using var fixture = new Fixture();

        await fixture.Service.ValidateRecoveryPlanForFailoverAsync(
            ServiceGroup,
            RecoveryPlan,
            ["westus2", "eastus2"],
            [RecoveryResourceId],
            "Approve",
            cancellationToken: TestContext.Current.CancellationToken);

        JsonElement root = ParseActionBody(fixture.Handler, PlanPath + "/validateForFailover");
        AssertFailoverPayload(root);
    }

    [Fact]
    public async Task Reprotect_SerializesGaPayloadNamesAndTypes()
    {
        using var fixture = new Fixture();

        await fixture.Service.ReprotectRecoveryPlanAsync(
            ServiceGroup,
            RecoveryPlan,
            [RecoveryResourceId],
            cancellationToken: TestContext.Current.CancellationToken);

        JsonElement root = ParseActionBody(fixture.Handler, PlanPath + "/reprotect");
        AssertReprotectPayload(root);
    }

    [Fact]
    public async Task ValidateForReprotect_SerializesGaPayloadNamesAndTypes()
    {
        using var fixture = new Fixture();

        await fixture.Service.ValidateRecoveryPlanForReprotectAsync(
            ServiceGroup,
            RecoveryPlan,
            [RecoveryResourceId],
            cancellationToken: TestContext.Current.CancellationToken);

        JsonElement root = ParseActionBody(fixture.Handler, PlanPath + "/validateForReprotect");
        AssertReprotectPayload(root);
    }

    [Fact]
    public async Task UpdateRecoveryResources_SerializesGaPayloadNamesAndTypes()
    {
        using var fixture = new Fixture();
        const string removedResource = "55555555-5555-5555-5555-555555555555";
        string removedResourceId = PlanPath + "/recoveryResources/" + removedResource;
        UpdateRecoveryResourcesContent content = RecoveryPlanUpdateResourcesCommand.CreateContent(
            new RecoveryPlanUpdateResourcesOptions
            {
                ServiceGroup = ServiceGroup,
                RecoveryPlan = RecoveryPlan,
                ResourcesToUpdate = $$"""
                    [{
                      "id":"{{RecoveryResourceId}}",
                      "properties":{
                        "recoveryResourceUniqueId":"{{RecoveryResource}}",
                        "inclusionState":"Excluded"
                      }
                    }]
                    """,
                ResourcesToRemove = $$"""["{{removedResourceId}}"]"""
            });

        await fixture.Service.UpdateRecoveryPlanResourcesAsync(
            ServiceGroup,
            RecoveryPlan,
            content,
            cancellationToken: TestContext.Current.CancellationToken);

        JsonElement root = ParseActionBody(fixture.Handler, PlanPath + "/updateResources");
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal(2, root.EnumerateObject().Count());
        JsonElement updates = root.GetProperty("resourcesToUpdate");
        JsonElement removals = root.GetProperty("resourcesToRemove");
        Assert.Equal(JsonValueKind.Array, updates.ValueKind);
        Assert.Equal(JsonValueKind.Array, removals.ValueKind);
        JsonElement update = Assert.Single(updates.EnumerateArray());
        Assert.Equal(JsonValueKind.Object, update.ValueKind);
        using JsonDocument expectedUpdate = JsonDocument.Parse($$"""
            {
              "properties":{
                "recoveryResourceUniqueId":"{{RecoveryResource}}",
                "inclusionState":"Excluded"
              }
            }
            """);
        Assert.True(JsonElement.DeepEquals(expectedUpdate.RootElement, update),
            $"Unexpected update resource JSON: {update.GetRawText()}");
        JsonElement removal = Assert.Single(removals.EnumerateArray());
        Assert.Equal(JsonValueKind.String, removal.ValueKind);
        Assert.Equal(removedResourceId, removal.GetString());
    }

    [Fact]
    public async Task ValidateForOperation_SerializesOperationNameAsString()
    {
        using var fixture = new Fixture();

        await fixture.Service.ValidateRecoveryPlanForOperationAsync(
            ServiceGroup,
            RecoveryPlan,
            RecoveryOperationName.Failover,
            cancellationToken: TestContext.Current.CancellationToken);

        JsonElement root = ParseActionBody(fixture.Handler, PlanPath + "/validateForOperation");
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal("operationName", Assert.Single(root.EnumerateObject()).Name);
        JsonElement operationName = root.GetProperty("operationName");
        Assert.Equal(JsonValueKind.String, operationName.ValueKind);
        Assert.Equal("Failover", operationName.GetString());
    }

    [Fact]
    public async Task ResumeRecoveryJob_WithDescription_SerializesDescriptionAsString()
    {
        using var fixture = new Fixture();
        const string description = "Continue after operator approval.";

        await fixture.Service.ResumeRecoveryJobAsync(
            ServiceGroup,
            RecoveryPlan,
            RecoveryJob,
            description,
            cancellationToken: TestContext.Current.CancellationToken);

        JsonElement root = ParseActionBody(fixture.Handler, JobPath + "/resume");
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal("description", Assert.Single(root.EnumerateObject()).Name);
        JsonElement serializedDescription = root.GetProperty("description");
        Assert.Equal(JsonValueKind.String, serializedDescription.ValueKind);
        Assert.Equal(description, serializedDescription.GetString());
    }

    [Fact]
    public async Task RetryRecoveryJob_SendsNoRequestBody()
    {
        using var fixture = new Fixture();

        await fixture.Service.RetryRecoveryJobAsync(
            ServiceGroup,
            RecoveryPlan,
            RecoveryJob,
            cancellationToken: TestContext.Current.CancellationToken);

        CapturedRequest request = Assert.Single(fixture.Handler.Requests, request =>
            request.Method == HttpMethod.Post && request.Uri.AbsolutePath == JobPath + "/retry");
        Assert.Null(request.Body);
        Assert.True(Guid.TryParse(request.OperationId, out _));
        Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(request.Uri.Query)["api-version"]);
    }

    [Fact]
    public async Task Failover_DeserializesRecoveryPlanActionBaseResult()
    {
        using var fixture = new Fixture();
        const string returnedJob = "66666666-6666-6666-6666-666666666666";
        fixture.Handler.ResponseFactory = (method, uri, _) =>
            method == HttpMethod.Post && uri.AbsolutePath == PlanPath + "/failover"
                ? JsonResponse($$"""{"jobId":"{{returnedJob}}"}""")
                : null;

        ArmClient client = await fixture.CreateArmClientAsync(TestContext.Current.CancellationToken);
        RecoveryPlanResource resource = client.GetRecoveryPlanResource(
            RecoveryPlanResource.CreateResourceIdentifier(ServiceGroup, RecoveryPlan));
        ArmOperation<RecoveryPlanActionBaseResult> operation = await resource.FailoverAsync(
            WaitUntil.Completed,
            Guid.NewGuid().ToString(),
            ResilienceManagementService.CreateFailoverRequestContent(
                ["westus2"],
                [RecoveryResourceId],
                userConsent: null),
            TestContext.Current.CancellationToken);

        Assert.True(operation.HasCompleted);
        Assert.Equal(returnedJob, operation.Value.JobId);
        CapturedRequest request = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(PlanPath + "/failover", request.Uri.AbsolutePath);
        Assert.Equal(HttpMethod.Post, request.Method);
    }

    [Fact]
    public async Task ValidateForFailover_DeserializesRecoveryResourceQualifications()
    {
        using var fixture = new Fixture();
        const string azureResourceId = "/subscriptions/11111111-1111-1111-1111-111111111111/resourceGroups/rg-ga/providers/Microsoft.Compute/virtualMachines/vm-ga";
        fixture.Handler.ResponseFactory = (method, uri, _) =>
            method == HttpMethod.Post && uri.AbsolutePath == PlanPath + "/validateForFailover"
                ? JsonResponse($$"""
                    {
                      "properties":{
                        "recoveryResourceQualifications":[{
                          "recoveryResource":{
                            "id":"{{RecoveryResourceId}}",
                            "name":"{{RecoveryResource}}",
                            "properties":{
                              "recoveryResourceUniqueId":"{{RecoveryResource}}",
                              "resourceId":"{{azureResourceId}}",
                              "resourceLocation":"westus2",
                              "resourcePhysicalZones":["westus2-az1"],
                              "inclusionState":"Included",
                              "protectionStatus":"Protected",
                              "needsAttention":true,
                              "attentionReasons":["Verify network mapping"]
                            }
                          },
                          "operationQualificationDetails":{
                            "qualificationState":"NotQualified",
                            "notQualifiedReasons":["Target capacity unavailable","Network mapping missing"]
                          }
                        }]
                      }
                    }
                    """)
                : null;

        RecoveryPlanValidateForFailoverResult result = await fixture.Service.ValidateRecoveryPlanForFailoverAsync(
            ServiceGroup,
            RecoveryPlan,
            ["westus2"],
            [RecoveryResourceId],
            cancellationToken: TestContext.Current.CancellationToken);

        RecoveryPlanFailoverQualification qualification = Assert.Single(result.RecoveryResourceQualifications);
        Assert.Equal(RecoveryResourceId, qualification.RecoveryResourceId);
        Assert.Equal(RecoveryResource, qualification.RecoveryResourceUniqueId);
        Assert.Equal(azureResourceId, qualification.AzureResourceId);
        Assert.Equal("westus2", qualification.AzureResourceLocation);
        Assert.Equal("NotQualified", qualification.QualificationState);
        Assert.Equal(["Target capacity unavailable", "Network mapping missing"], qualification.NotQualifiedReasons);
        Assert.Equal(["westus2-az1"], qualification.ResourcePhysicalZones);
        Assert.Equal("Included", qualification.InclusionState);
        Assert.Equal("Protected", qualification.ProtectionStatus);
        Assert.True(qualification.IsAttentionRequired);
        Assert.Equal(["Verify network mapping"], qualification.AttentionReasons);
    }

    [Fact]
    public async Task GetRecoveryJob_DeserializesResilienceManagementJobDetails()
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseFactory = (method, uri, _) =>
            method == HttpMethod.Get && uri.AbsolutePath == JobPath
                ? JsonResponse(RepresentativeRecoveryJobJson)
                : null;

        JsonElement result = await fixture.Service.GetRecoveryJobAsync(
            ServiceGroup,
            RecoveryPlan,
            RecoveryJob,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryJob, result.GetProperty("name").GetString());
        JsonElement properties = result.GetProperty("properties");
        Assert.Equal("Failed", properties.GetProperty("status").GetString());
        Assert.Equal("Failover", properties.GetProperty("operation").GetString());
        JsonElement extended = properties.GetProperty("jobExtendedInfo");
        Assert.Equal("A provider task requires attention.", extended.GetProperty("dynamicErrorMessage").GetString());
        JsonElement task = Assert.Single(extended.GetProperty("tasksList").EnumerateArray());
        Assert.Equal("Configure target", task.GetProperty("taskName").GetString());
        Assert.Equal("Failed", task.GetProperty("status").GetString());
        Assert.Equal("linked-job-ga", Assert.Single(task.GetProperty("linkedJobIds").EnumerateArray()).GetString());
        Assert.Equal("Operator acknowledged the failure.", Assert.Single(task.GetProperty("comments").EnumerateArray()).GetString());
        Assert.Equal(2, Assert.Single(task.GetProperty("retryDetails").EnumerateArray()).GetProperty("retryCount").GetInt32());
        JsonElement subTask = Assert.Single(task.GetProperty("subTasksList").EnumerateArray());
        Assert.Equal("Validate network", subTask.GetProperty("taskName").GetString());
        Assert.Equal("Succeeded", subTask.GetProperty("status").GetString());
        JsonElement error = task.GetProperty("errorDetails");
        Assert.Equal("NetworkMappingMissing", error.GetProperty("errorCode").GetString());
        Assert.Equal("A network mapping is required.", error.GetProperty("errorMessage").GetString());
        Assert.Equal("Create the target network mapping.", Assert.Single(error.GetProperty("recommendations").EnumerateArray()).GetString());
    }

    [Fact]
    public async Task GetRecoveryJobTarget_DeserializesRecoveryJobResourceProperties()
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseFactory = (method, uri, _) =>
            method == HttpMethod.Get && uri.AbsolutePath == TargetPath
                ? JsonResponse(RepresentativeRecoveryJobTargetJson)
                : null;

        JsonElement result = await fixture.Service.GetRecoveryJobResourceAsync(
            ServiceGroup,
            RecoveryPlan,
            RecoveryJob,
            RecoveryJobTarget,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(RecoveryJobTarget, result.GetProperty("name").GetString());
        JsonElement properties = result.GetProperty("properties");
        Assert.Equal(RecoveryResourceId, properties.GetProperty("resourceId").GetString());
        Assert.Equal("Failed", properties.GetProperty("status").GetString());
        JsonElement extended = properties.GetProperty("jobExtendedInfo");
        Assert.Equal("Target operation failed.", extended.GetProperty("dynamicErrorMessage").GetString());
        JsonElement task = Assert.Single(extended.GetProperty("tasksList").EnumerateArray());
        Assert.Equal("Protect disks", task.GetProperty("taskName").GetString());
        Assert.Equal("DiskProtectionFailed", task.GetProperty("errorDetails").GetProperty("errorCode").GetString());
        Assert.Equal("Retry after fixing disk access.", Assert.Single(
            task.GetProperty("errorDetails").GetProperty("recommendations").EnumerateArray()).GetString());
    }

    private static JsonElement ParseActionBody(CapturingHandler handler, string path)
    {
        CapturedRequest request = Assert.Single(handler.Requests, request =>
            request.Method == HttpMethod.Post && request.Uri.AbsolutePath == path);
        Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(request.Uri.Query)["api-version"]);
        using JsonDocument document = JsonDocument.Parse(Assert.IsType<string>(request.Body));
        return document.RootElement.Clone();
    }

    private static void AssertFailoverPayload(JsonElement root)
    {
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal(2, root.EnumerateObject().Count());
        JsonElement direction = root.GetProperty("failoverDirection");
        Assert.Equal(JsonValueKind.String, direction.ValueKind);
        Assert.Equal("FromSpecificLocations", direction.GetString());
        JsonElement properties = root.GetProperty("failoverRequestProperties");
        Assert.Equal(JsonValueKind.Object, properties.ValueKind);
        Assert.Equal(3, properties.EnumerateObject().Count());
        AssertStringArray(properties.GetProperty("sourceLocations"), "westus2", "eastus2");
        AssertStringArray(properties.GetProperty("selectedResourceIds"), RecoveryResourceId);
        JsonElement executionConfigurations = properties.GetProperty("executionConfigurations");
        Assert.Equal(JsonValueKind.Object, executionConfigurations.ValueKind);
        Assert.Equal("userConsent", Assert.Single(executionConfigurations.EnumerateObject()).Name);
        JsonElement consent = executionConfigurations.GetProperty("userConsent");
        Assert.Equal(JsonValueKind.String, consent.ValueKind);
        Assert.Equal("Approve", consent.GetString());
        Assert.False(properties.TryGetProperty("executionConfigurationsUserConsent", out _));
        Assert.False(root.TryGetProperty("failoverProperties", out _));
        Assert.False(root.TryGetProperty("sourceLocations", out _));
        Assert.False(root.TryGetProperty("selectedResourceIds", out _));
    }

    private static void AssertReprotectPayload(JsonElement root)
    {
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal("reprotectRequestProperties", Assert.Single(root.EnumerateObject()).Name);
        JsonElement properties = root.GetProperty("reprotectRequestProperties");
        Assert.Equal(JsonValueKind.Object, properties.ValueKind);
        Assert.Equal("selectedResourceIds", Assert.Single(properties.EnumerateObject()).Name);
        AssertStringArray(properties.GetProperty("selectedResourceIds"), RecoveryResourceId);
        Assert.False(root.TryGetProperty("reprotectProperties", out _));
        Assert.False(root.TryGetProperty("selectedResourceIds", out _));
    }

    private static void AssertStringArray(JsonElement element, params string[] expected)
    {
        Assert.Equal(JsonValueKind.Array, element.ValueKind);
        JsonElement[] values = element.EnumerateArray().ToArray();
        Assert.Equal(expected.Length, values.Length);
        Assert.All(values, value => Assert.Equal(JsonValueKind.String, value.ValueKind));
        Assert.Equal(expected, values.Select(value => value.GetString()).ToArray());
    }

    private static HttpResponseMessage JsonResponse(string body, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private static string RepresentativeRecoveryJobJson => $$"""
        {
          "id":"{{JobPath}}",
          "name":"{{RecoveryJob}}",
          "type":"Microsoft.AzureResilienceManagement/recoveryPlans/recoveryJobs",
          "properties":{
            "status":"Failed",
            "operation":"Failover",
            "provisioningState":"Succeeded",
            "jobExtendedInfo":{
              "dynamicErrorMessage":"A provider task requires attention.",
              "tasksList":[{
                "taskName":"Configure target",
                "status":"Failed",
                "linkedJobIds":["linked-job-ga"],
                "comments":["Operator acknowledged the failure."],
                "retryDetails":[{"retryCount":2,"isRetryable":true}],
                "subTasksList":[{"taskName":"Validate network","status":"Succeeded","subTasksList":[]}],
                "errorDetails":{
                  "errorCode":"NetworkMappingMissing",
                  "errorMessage":"A network mapping is required.",
                  "recommendations":["Create the target network mapping."]
                }
              }]
            }
          }
        }
        """;

    private static string RepresentativeRecoveryJobTargetJson => $$"""
        {
          "id":"{{TargetPath}}",
          "name":"{{RecoveryJobTarget}}",
          "type":"Microsoft.AzureResilienceManagement/recoveryPlans/recoveryJobs/recoveryJobResources",
          "properties":{
            "status":"Failed",
            "resourceId":"{{RecoveryResourceId}}",
            "provisioningState":"Succeeded",
            "jobExtendedInfo":{
              "dynamicErrorMessage":"Target operation failed.",
              "tasksList":[{
                "taskName":"Protect disks",
                "status":"Failed",
                "subTasksList":[],
                "errorDetails":{
                  "errorCode":"DiskProtectionFailed",
                  "errorMessage":"Disk protection could not start.",
                  "recommendations":["Retry after fixing disk access."]
                }
              }]
            }
          }
        }
        """;

    [Fact]
    public async Task RecoveryAction_WhenArmReturnsError_PropagatesStatusAndErrorCode()
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseFactory = (method, uri, _) =>
            method == HttpMethod.Post && uri.AbsolutePath == PlanPath + "/failover"
                ? JsonResponse(
                    """{"error":{"code":"RecoveryPlanConflict","message":"The recovery action conflicts with the current state."}}""",
                    HttpStatusCode.Conflict)
                : null;

        RequestFailedException exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            fixture.Service.FailoverRecoveryPlanAsync(
                ServiceGroup,
                RecoveryPlan,
                ["westus2"],
                [RecoveryResourceId],
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.Conflict, exception.Status);
        Assert.Equal("RecoveryPlanConflict", exception.ErrorCode);
        AssertNonRevealingException(exception);
        CapturedRequest failedRequest = Assert.Single(fixture.Handler.Requests, request =>
            request.Method == HttpMethod.Post);
        Assert.Equal("management.azure.com", failedRequest.Uri.Host);
        Assert.Equal(PlanPath + "/failover", failedRequest.Uri.AbsolutePath);
        Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(failedRequest.Uri.Query)["api-version"]);
    }

    [Fact]
    public async Task RecoveryItemGet_WhenArmReturnsNotFound_PropagatesStatusAndErrorCode()
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseFactory = (method, uri, _) =>
            method == HttpMethod.Get && uri.AbsolutePath == TargetPath
                ? JsonResponse(
                    """{"error":{"code":"RecoveryJobResourceNotFound","message":"The requested recovery job resource was not found."}}""",
                    HttpStatusCode.NotFound)
                : null;

        RequestFailedException exception = await Assert.ThrowsAsync<RequestFailedException>(() =>
            fixture.Service.GetRecoveryJobResourceAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryJob,
                RecoveryJobTarget,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.NotFound, exception.Status);
        Assert.Equal("RecoveryJobResourceNotFound", exception.ErrorCode);
        AssertNonRevealingException(exception);
        CapturedRequest failedRequest = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Get, failedRequest.Method);
        Assert.Equal("management.azure.com", failedRequest.Uri.Host);
        Assert.Equal(TargetPath, failedRequest.Uri.AbsolutePath);
        Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(failedRequest.Uri.Query)["api-version"]);
    }

    [Fact]
    public async Task RecoveryResponse_WhenJsonIsMalformed_PropagatesDeserializationFailure()
    {
        using var fixture = new Fixture();
        fixture.Handler.ResponseFactory = (method, uri, _) =>
            method == HttpMethod.Get && uri.AbsolutePath == JobPath
                ? JsonResponse("""{"id":""")
                : null;

        JsonException exception = await Assert.ThrowsAnyAsync<JsonException>(() =>
            fixture.Service.GetRecoveryJobAsync(
                ServiceGroup,
                RecoveryPlan,
                RecoveryJob,
                cancellationToken: TestContext.Current.CancellationToken));

        AssertNonRevealingException(exception);
        CapturedRequest failedRequest = Assert.Single(fixture.Handler.Requests);
        Assert.Equal(HttpMethod.Get, failedRequest.Method);
        Assert.Equal("management.azure.com", failedRequest.Uri.Host);
        Assert.Equal(JobPath, failedRequest.Uri.AbsolutePath);
        Assert.Equal("2026-10-01", HttpUtility.ParseQueryString(failedRequest.Uri.Query)["api-version"]);
    }

    private static void AssertNonRevealingException(Exception exception)
    {
        Assert.DoesNotContain("unit-test-token", exception.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Authorization", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RecoveryPlanFailoverOptions", exception.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("RecoveryJobResourceGetOptions", exception.ToString(), StringComparison.Ordinal);
    }
}
