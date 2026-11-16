// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.Assessment;

public class AssessmentCommandTests
{
    private const string SasUri = "https://sa.blob.core.windows.net/zips/collector.zip?sp=r&sig=secret";

    private sealed class ManualTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            var response = new HttpResponseMessage(status) { Content = new StringContent(body) };
            response.Headers.Add("Operation-Id", "11111111-1111-1111-1111-111111111111");
            return response;
        }
    }

    [Theory]
    [InlineData("assess-1", true)]
    [InlineData("A_b-9", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("-starts-with-dash", false)]
    [InlineData("has space", false)]
    public void IsValidAssessmentName(string? name, bool expected) =>
        Assert.Equal(expected, CreateAssessmentCommand.IsValidAssessmentName(name));

    [Fact]
    public void IsValidAssessmentName_RejectsMoreThan64Characters()
    {
        Assert.True(CreateAssessmentCommand.IsValidAssessmentName(new string('a', 64)));
        Assert.False(CreateAssessmentCommand.IsValidAssessmentName(new string('a', 65)));
    }

    [Theory]
    [InlineData("eastus", "eastus")]
    [InlineData(" East US ", "eastus")]
    [InlineData("WestEurope", "westeurope")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("east-us", null)]
    [InlineData("1eastus", null)]
    public void NormalizeRegion(string? region, string? expected) =>
        Assert.Equal(expected, CreateAssessmentCommand.NormalizeRegion(region));

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData(" EUR ", "EUR")]
    [InlineData("cny", "CNY")]
    [InlineData(null, null)]
    [InlineData("US", null)]
    [InlineData("XYZ", null)]
    [InlineData("PLN", null)]
    public void NormalizeCurrency_AcceptsOnlySupportedCodes(string? currency, string? expected) =>
        Assert.Equal(expected, CreateAssessmentCommand.NormalizeCurrency(currency));

    [Theory]
    [InlineData(SasUri, true)]
    [InlineData("http://127.0.0.1:10000/devstoreaccount1/zips/c.zip?sig=x", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("collector.zip", false)]
    [InlineData("ftp://host/collector.zip", false)]
    public void IsValidInventorySasUri(string? uri, bool expected) =>
        Assert.Equal(expected, CreateAssessmentCommand.IsValidInventorySasUri(uri));

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(4, 16)]
    [InlineData(9, 512)]
    public void GetPollDelay_DoublesFromOneSecond(int attempt, int expectedSeconds) =>
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), CreateAssessmentCommand.GetPollDelay(attempt));

    [Fact]
    public async Task Create_WaitsForCompletionThenStartsHaSession()
    {
        var (command, time, ha) = CreateCommand(new MockAssessmentServiceClient(Time));

        var result = await Run(command, time);

        Assert.True(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal("eastus", result.TargetRegion);
        Assert.Equal("USD", result.Currency);
        Assert.StartsWith("https://assessment.mock.local/assessment-output/", result.AssessmentOutputUrl);
        Assert.Equal($"azcopy copy \"{result.AssessmentOutputUrl}\" \"<path to save the file>\" --from-to=BlobLocal", result.AssessmentDownloadCommand);
        Assert.Null(result.HaFailedStep);
        Assert.Equal("ha-1", result.HaSessionId);
        Assert.Equal("op-1", result.HaOperationId);
        Assert.Equal(
            "azcopy copy \"<path to your file>\" \"https://sa.blob.core.windows.net/c/ha-1/input?sig=x\" --from-to=LocalBlob --blob-type=BlockBlob --overwrite=true --check-length=false",
            result.UploadCommand);
        Assert.Contains("single zip", result.Instructions);
        await ha.Received(1).StartAsync("token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_SendsSasUriWithoutWorkspaceId()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        client.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(400, null, null, null, null, "InvalidRequest", "Bad."));
        var (command, time, _) = CreateCommand(client);

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.Contains("InvalidRequest", result.Error);
        await client.Received(1).CreateAssessmentAsync(
            Arg.Is<AssessmentRequest>(r => r.AssessmentName == "assess-1" && r.WorkspaceId == null && r.InventorySasUri == SasUri
                && r.TargetRegion == "eastus" && r.Currency == "USD"),
            "token",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_StillRunningAfterTenPolls_ReturnsRunningAndSkipsHa()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        Guid id = Guid.NewGuid();
        client.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(202, id, "Running", null, 5, null, null));
        client.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(200, id, "Running", null, 5, null, null));
        var (command, time, ha) = CreateCommand(client);
        DateTimeOffset start = time.Now;

        var result = await Run(command, time);

        Assert.True(result.Success);
        Assert.False(result.IsCompleted);
        Assert.Equal("Running", result.Status);
        Assert.Equal(id.ToString("D"), result.OperationId);
        Assert.Null(result.HaSessionId);
        Assert.Null(result.UploadCommand);
        Assert.Contains("still running", result.Instructions);
        Assert.Equal(TimeSpan.FromSeconds(1023), time.Now - start);
        await client.Received(10).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
        await ha.DidNotReceiveWithAnyArgs().StartAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_TransientStatusError_IsRetried()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        Guid id = Guid.NewGuid();
        client.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(202, id, "Running", null, 5, null, null));
        client.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(503, id, null, null, null, null, "Busy."),
            new AssessmentOperation(null, id, null, null, null, null, "Timeout."),
            new AssessmentOperation(200, id, "Succeeded", "https://sa.blob.core.windows.net/out/r.json?sig=x", null, null, null));
        var (command, time, ha) = CreateCommand(client);

        var result = await Run(command, time);

        Assert.True(result.Success);
        Assert.Equal("Succeeded", result.Status);
        await client.Received(3).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
        await ha.Received(1).StartAsync("token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_OperationNotFound_FailsWithoutRetry()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        Guid id = Guid.NewGuid();
        client.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(202, id, "Running", null, 5, null, null));
        client.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(404, id, null, null, null, "OperationNotFound", "Not found."));
        var (command, time, _) = CreateCommand(client);

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.Equal(404, result.HttpStatus);
        Assert.Contains("OperationNotFound", result.Error);
        await client.Received(1).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_AssessmentFails_SkipsHa()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        Guid id = Guid.NewGuid();
        client.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(202, id, "Running", null, 5, null, null));
        client.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(200, id, "Failed", null, null, "Boom", "It failed."));
        var (command, time, ha) = CreateCommand(client);

        var result = await Run(command, time);

        Assert.True(result.IsCompleted);
        Assert.Equal("Failed", result.Status);
        Assert.Equal("It failed.", result.ErrorMessage);
        Assert.Null(result.AssessmentDownloadCommand);
        Assert.Contains("did not complete", result.Instructions);
        await ha.DidNotReceiveWithAnyArgs().StartAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_HaSessionFails_ReturnsError()
    {
        var (command, time, ha) = CreateCommand(new MockAssessmentServiceClient(Time));
        ha.StartAsync(default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult("createSession", 400, "ha-1", false, null, "req", null, "Unknown skill type."));

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Equal("createSession", result.HaFailedStep);
        Assert.Equal(400, result.HaHttpStatus);
        Assert.Null(result.UploadCommand);
        Assert.NotNull(result.AssessmentDownloadCommand);
        Assert.Contains("failed at createSession", result.Error);
    }

    [Fact]
    public async Task Create_HaInvokeReturnsInvalidUrl_ReturnsError()
    {
        var (command, time, ha) = CreateCommand(new MockAssessmentServiceClient(Time));
        ha.StartAsync(default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult(null, 200, "ha-1", false, "op-1", "req", "not-a-url", null));

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.Equal("invokeSession", result.HaFailedStep);
        Assert.Null(result.UploadCommand);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("collector.zip")]
    public async Task Create_InvalidInventorySasUri_FailsBeforeCallingService(string? sasUri)
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        var (command, _, _) = CreateCommand(client);
        CreateAssessmentOptions options = ValidOptions();
        options.InventorySasUri = sasUri;

        var result = await command.RunAsync(options, (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains("inventory-sas-uri", result.Error);
        await client.DidNotReceiveWithAnyArgs().CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_WithoutToken_FailsBeforeCallingService()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        var command = new CreateAssessmentCommand(client, Substitute.For<IHaSessionClient>(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        var result = await command.RunAsync(ValidOptions(), (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains("No bearer token", result.Error);
        await client.DidNotReceiveWithAnyArgs().CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    private ManualTimeProvider Time { get; } = new();

    private static CreateAssessmentOptions ValidOptions() =>
        new() { AssessmentName = "assess-1", TargetRegion = "East US", Currency = "usd", InventorySasUri = SasUri };

    private static AssessmentRequest Request(string? workspaceId = null) => new("assess-1", workspaceId, SasUri, "eastus", "USD");

    private (CreateAssessmentCommand Command, ManualTimeProvider Time, IHaSessionClient Ha) CreateCommand(IAssessmentServiceClient client)
    {
        var ha = Substitute.For<IHaSessionClient>();
        ha.StartAsync(default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult(null, 200, "ha-1", false, "op-1", "req", "https://sa.blob.core.windows.net/c/ha-1/input?sig=x", null));
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";
        return (new CreateAssessmentCommand(client, ha, new HttpContextAccessor { HttpContext = context }), Time, ha);
    }

    private static Task<CreateAssessmentCommand.CreateAssessmentCommandResult> Run(CreateAssessmentCommand command, ManualTimeProvider time) =>
        command.RunAsync(ValidOptions(), (delay, _) => { time.Now += delay; return Task.CompletedTask; }, TestContext.Current.CancellationToken);

    [Fact]
    public async Task MockClient_ReportsRunningThenSucceededWithOutput()
    {
        var time = new ManualTimeProvider();
        var client = new MockAssessmentServiceClient(time);

        AssessmentOperation created = await client.CreateAssessmentAsync(Request(), "token", TestContext.Current.CancellationToken);
        Assert.Equal(202, created.HttpStatus);
        Assert.Equal("Running", created.Status);
        Assert.NotNull(created.OperationId);

        time.Now += TimeSpan.FromSeconds(10);
        AssessmentOperation running = await client.GetOperationAsync(created.OperationId!.Value, null, "token", TestContext.Current.CancellationToken);
        Assert.Equal("Running", running.Status);
        Assert.Equal(20, running.RetryAfterSeconds);
        Assert.Null(running.OutputUrl);

        time.Now += TimeSpan.FromSeconds(20);
        AssessmentOperation done = await client.GetOperationAsync(created.OperationId.Value, null, "token", TestContext.Current.CancellationToken);
        Assert.Equal(200, done.HttpStatus);
        Assert.Equal("Succeeded", done.Status);
        Assert.Null(done.RetryAfterSeconds);
        Assert.Equal("https://assessment.mock.local/assessment-output/default/assess-1-recommendations.json?sp=r&sig=mock", done.OutputUrl);
    }

    [Fact]
    public async Task MockClient_ReturnsNotFoundForUnknownOperationOrOtherWorkspace()
    {
        var client = new MockAssessmentServiceClient(new ManualTimeProvider());
        AssessmentOperation created = await client.CreateAssessmentAsync(Request(), "token", TestContext.Current.CancellationToken);

        AssessmentOperation unknown = await client.GetOperationAsync(Guid.NewGuid(), null, "token", TestContext.Current.CancellationToken);
        AssessmentOperation otherWorkspace = await client.GetOperationAsync(created.OperationId!.Value, "ws-2", "token", TestContext.Current.CancellationToken);

        Assert.Equal(404, unknown.HttpStatus);
        Assert.Equal(404, otherWorkspace.HttpStatus);
        Assert.Equal("OperationNotFound", otherWorkspace.ErrorCode);
    }

    [Fact]
    public void AssessmentRequest_ToStringDoesNotContainSasUri() =>
        Assert.DoesNotContain("sig=secret", Request().ToString());

    [Fact]
    public void BusinessCaseClient_BuildsContractUrls()
    {
        Guid id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        Assert.Equal(
            "https://bc.local/businessCaseRestApi/machineAssessmentsFromZip/assess%201?api-version=2022-01-05-preview",
            BusinessCaseAssessmentServiceClient.CreateUrl("https://bc.local", "assess 1"));
        Assert.Equal(
            $"https://bc.local/businessCaseRestApi/machineAssessmentsFromZip/operations/{id:D}?api-version=2022-01-05-preview",
            BusinessCaseAssessmentServiceClient.OperationUrl("https://bc.local", id, null));
        Assert.Equal(
            $"https://bc.local/businessCaseRestApi/machineAssessmentsFromZip/operations/{id:D}?api-version=2022-01-05-preview&workspaceId=ws%201",
            BusinessCaseAssessmentServiceClient.OperationUrl("https://bc.local", id, "ws 1"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ws-1")]
    public void BusinessCaseClient_BuildsCreateBody(string? workspaceId)
    {
        using JsonDocument body = JsonDocument.Parse(BusinessCaseAssessmentServiceClient.BuildCreateBody(Request(workspaceId)));
        JsonElement root = body.RootElement;

        Assert.Equal(workspaceId is not null, root.TryGetProperty("workspaceId", out JsonElement ws));
        if (workspaceId is not null)
        {
            Assert.Equal(workspaceId, ws.GetString());
        }

        Assert.Equal(SasUri, root.GetProperty("inventorySasUri").GetString());
        Assert.Equal("eastus", root.GetProperty("settings").GetProperty("azureLocation").GetString());
        Assert.Equal("USD", root.GetProperty("settings").GetProperty("currency").GetString());
    }

    [Fact]
    public void BusinessCaseClient_ParsesSucceededOperation()
    {
        Guid id = Guid.NewGuid();
        var result = BusinessCaseAssessmentServiceClient.Parse(
            new(200, $$"""{"id":"{{id}}","status":"Succeeded","output":"https://sa/out.json?sig=x"}""", null, null, null), id);

        Assert.Equal(id, result.OperationId);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal("https://sa/out.json?sig=x", result.OutputUrl);
    }

    [Fact]
    public void BusinessCaseClient_ParsesFailedOperation()
    {
        var result = BusinessCaseAssessmentServiceClient.Parse(
            new(200, """{"status":"Failed","output":"ignored","error":{"code":"InvalidZip","message":"Bad zip."}}""", null, null, null), Guid.NewGuid());

        Assert.Equal("Failed", result.Status);
        Assert.Null(result.OutputUrl);
        Assert.Equal("InvalidZip", result.ErrorCode);
        Assert.Equal("Bad zip.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("""{"error":{"code":"OperationNotFound","message":"Not found."}}""", "OperationNotFound", "Not found.")]
    [InlineData("""{"message":"Forbidden."}""", null, "Forbidden.")]
    [InlineData("plain text", null, "plain text")]
    public void BusinessCaseClient_ParsesErrorResponse(string body, string? code, string message)
    {
        Guid id = Guid.NewGuid();
        var result = BusinessCaseAssessmentServiceClient.Parse(new(404, body, null, null, null), id);

        Assert.Equal(404, result.HttpStatus);
        Assert.Equal(id, result.OperationId);
        Assert.Null(result.Status);
        Assert.Equal(code, result.ErrorCode);
        Assert.Equal(message, result.ErrorMessage);
    }

    [Fact]
    public async Task BusinessCaseClient_CreateSendsPutWithTokenAndUsesOperationIdHeader()
    {
        var handler = new StubHandler(HttpStatusCode.Accepted, """{"status":"Running"}""");
        var client = new BusinessCaseAssessmentServiceClient(
            NullLogger<BusinessCaseAssessmentServiceClient>.Instance, new HttpClient(handler), "https://bc.local/");

        AssessmentOperation result = await client.CreateAssessmentAsync(Request(), "token", TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Put, handler.Request!.Method);
        Assert.Equal(
            "https://bc.local/businessCaseRestApi/machineAssessmentsFromZip/assess-1?api-version=2022-01-05-preview",
            handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer token", handler.Request.Headers.Authorization!.ToString());
        using (JsonDocument sent = JsonDocument.Parse(handler.RequestBody!))
        {
            Assert.Equal(SasUri, sent.RootElement.GetProperty("inventorySasUri").GetString());
        }
        Assert.Equal(202, result.HttpStatus);
        Assert.Equal("Running", result.Status);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), result.OperationId);
    }

    [Fact]
    public void ParseStatus_Running_IsNotCompletedWithRetry()
    {
        var result = GetAssessmentStatusCommand.Parse("a", "op", new(200, Guid.NewGuid(), "Running", null, 15, null, null));

        Assert.True(result.Success);
        Assert.False(result.IsCompleted);
        Assert.Equal(15, result.RetryAfterSeconds);
        Assert.Null(result.AssessmentDownloadCommand);
        Assert.Contains("still running", result.Instructions);
    }

    [Fact]
    public void ParseStatus_Succeeded_ReturnsDownloadCommand()
    {
        var result = GetAssessmentStatusCommand.Parse(null, "op", new(200, Guid.NewGuid(), "Succeeded", "https://sa/out.json?sig=x", null, null, null));

        Assert.True(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Null(result.RetryAfterSeconds);
        Assert.Equal("https://sa/out.json?sig=x", result.AssessmentOutputUrl);
        Assert.Equal("azcopy copy \"https://sa/out.json?sig=x\" \"<path to save the file>\" --from-to=BlobLocal", result.AssessmentDownloadCommand);
        Assert.Contains("completed", result.Instructions);
    }

    [Fact]
    public void ParseStatus_Failed_IsCompletedWithError()
    {
        var result = GetAssessmentStatusCommand.Parse("a", "op", new(200, Guid.NewGuid(), "Failed", null, null, "Boom", "It failed."));

        Assert.True(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Equal("It failed.", result.ErrorMessage);
        Assert.Contains("did not complete", result.Instructions);
    }

    [Fact]
    public void ParseStatus_NotFound_Fails()
    {
        var result = GetAssessmentStatusCommand.Parse("a", "op", new(404, Guid.NewGuid(), null, null, null, "OperationNotFound", null));

        Assert.False(result.Success);
        Assert.Equal(404, result.HttpStatus);
        Assert.Contains("No assessment operation", result.Error);
    }

    [Fact]
    public async Task Status_CallsServiceWithoutAssessmentName()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        Guid id = Guid.NewGuid();
        client.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(200, id, "Running", null, 5, null, null));
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";
        var command = new GetAssessmentStatusCommand(client, new HttpContextAccessor { HttpContext = context });

        var result = await command.RunAsync(new() { OperationId = id.ToString() }, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Null(result.AssessmentName);
        await client.Received(1).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
    }
}