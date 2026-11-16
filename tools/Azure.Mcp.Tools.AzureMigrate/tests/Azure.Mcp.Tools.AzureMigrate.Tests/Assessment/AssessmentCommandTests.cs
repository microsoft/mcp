// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
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
    private const string OutputUri = "https://sa.blob.core.windows.net/out/recommendations.json?sp=r&sig=out";
    private const string HaWriteSas = "https://sa.blob.core.windows.net/c/ha-1/input?sp=cw&sig=write";

    private sealed class ManualTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class SequenceHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private int _calls;

        public List<HttpRequestMessage> Requests { get; } = [];
        public List<byte[]> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null ? [] : await request.Content.ReadAsByteArrayAsync(cancellationToken));
            HttpStatusCode status = statuses[Math.Min(_calls++, statuses.Length - 1)];
            return new HttpResponseMessage(status) { Content = new StringContent(status == HttpStatusCode.Created ? string.Empty : "error") };
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
    public async Task Create_WaitsForCompletionThenStartsHaWithInventoryAndAssessmentSas()
    {
        Guid id = Guid.NewGuid();
        var (command, time, create, status, ha) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id, 202));
        status.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            Running(id), new AssessmentOperation(200, id, "Succeeded", OutputUri, null, null, null));

        var result = await Run(command, time);

        Assert.True(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal("eastus", result.TargetRegion);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(OutputUri, result.AssessmentOutputUrl);
        Assert.Equal($"azcopy copy \"{OutputUri}\" \"<path to save the file>\" --from-to=BlobLocal", result.AssessmentDownloadCommand);
        Assert.Null(result.HaFailedStep);
        Assert.Equal("ha-1", result.HaSessionId);
        Assert.Equal("op-1", result.HaOperationId);
        Assert.Contains("do not need to upload anything", result.Instructions);
        await ha.Received(1).StartAsync("token", new HaInput(SasUri, OutputUri), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_SendsSasUriWithoutWorkspaceId()
    {
        var (command, time, create, _, _) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(400, null, null, null, null, "InvalidRequest", "Bad."));

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.Contains("InvalidRequest", result.Error);
        await create.Received(1).CreateAssessmentAsync(
            Arg.Is<AssessmentRequest>(r => r.AssessmentName == "assess-1" && r.WorkspaceId == null && r.InventorySasUri == SasUri
                && r.TargetRegion == "eastus" && r.Currency == "USD"),
            "token",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_StillRunningAfterTenPolls_ReturnsRunningAndSkipsHa()
    {
        Guid id = Guid.NewGuid();
        var (command, time, create, status, ha) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id, 202));
        status.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id));
        DateTimeOffset start = time.Now;

        var result = await Run(command, time);

        Assert.True(result.Success);
        Assert.False(result.IsCompleted);
        Assert.Equal("Running", result.Status);
        Assert.Equal(id.ToString("D"), result.OperationId);
        Assert.Null(result.HaSessionId);
        Assert.Contains("still running", result.Instructions);
        Assert.Equal(TimeSpan.FromSeconds(1023), time.Now - start);
        await status.Received(10).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
        await ha.DidNotReceiveWithAnyArgs().StartAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_TransientStatusError_IsRetried()
    {
        Guid id = Guid.NewGuid();
        var (command, time, create, status, ha) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id, 202));
        status.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(503, id, null, null, null, null, "Busy."),
            new AssessmentOperation(null, id, null, null, null, null, "Timeout."),
            new AssessmentOperation(200, id, "Succeeded", OutputUri, null, null, null));

        var result = await Run(command, time);

        Assert.True(result.Success);
        Assert.Equal("Succeeded", result.Status);
        await status.Received(3).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
        await ha.Received(1).StartAsync("token", Arg.Any<HaInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_OperationNotFound_FailsWithoutRetry()
    {
        Guid id = Guid.NewGuid();
        var (command, time, create, status, _) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id, 202));
        status.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(404, id, null, null, null, "OperationNotFound", "Not found."));

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.Equal(404, result.HttpStatus);
        Assert.Contains("OperationNotFound", result.Error);
        await status.Received(1).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_AssessmentFails_SkipsHa()
    {
        Guid id = Guid.NewGuid();
        var (command, time, create, status, ha) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id, 202));
        status.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(200, id, "Failed", null, null, "Boom", "It failed."));

        var result = await Run(command, time);

        Assert.True(result.IsCompleted);
        Assert.Equal("Failed", result.Status);
        Assert.Equal("It failed.", result.ErrorMessage);
        Assert.Null(result.AssessmentDownloadCommand);
        Assert.Contains("did not complete", result.Instructions);
        await ha.DidNotReceiveWithAnyArgs().StartAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_SucceededWithoutOutputUrl_SkipsHa()
    {
        Guid id = Guid.NewGuid();
        var (command, time, create, status, ha) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id, 202));
        status.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(200, id, "Succeeded", null, null, null, null));

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.Equal("assessmentOutput", result.HaFailedStep);
        await ha.DidNotReceiveWithAnyArgs().StartAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("createSession")]
    [InlineData("uploadInput")]
    public async Task Create_HaStartFails_ReturnsError(string failedStep)
    {
        Guid id = Guid.NewGuid();
        var (command, time, create, status, ha) = CreateCommand();
        create.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new AssessmentOperation(200, id, "Succeeded", OutputUri, null, null, null));
        ha.StartAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult(failedStep, 400, "ha-1", false, null, "req", "Rejected."));

        var result = await Run(command, time);

        Assert.False(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Equal(failedStep, result.HaFailedStep);
        Assert.Equal(400, result.HaHttpStatus);
        Assert.NotNull(result.AssessmentDownloadCommand);
        Assert.Contains($"failed at {failedStep}", result.Error);
        await status.DidNotReceiveWithAnyArgs().GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("collector.zip")]
    public async Task Create_InvalidInventorySasUri_FailsBeforeCallingService(string? sasUri)
    {
        var (command, _, create, _, _) = CreateCommand();
        CreateAssessmentOptions options = ValidOptions();
        options.InventorySasUri = sasUri;

        var result = await command.RunAsync(options, (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains("inventory-sas-uri", result.Error);
        await create.DidNotReceiveWithAnyArgs().CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_WithoutToken_FailsBeforeCallingService()
    {
        var create = Substitute.For<IAssessmentCreateClient>();
        var command = new CreateAssessmentCommand(
            create, Substitute.For<IAssessmentOperationStatusClient>(), Substitute.For<IHaSessionClient>(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        var result = await command.RunAsync(ValidOptions(), (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains("No bearer token", result.Error);
        await create.DidNotReceiveWithAnyArgs().CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public void AssessmentRequest_ToStringDoesNotContainSasUri() =>
        Assert.DoesNotContain("sig=secret", new AssessmentRequest("assess-1", null, SasUri, "eastus", "USD").ToString());

    [Fact]
    public void HaInputPackage_BuildsZipWithManifestOnly()
    {
        byte[] zip = HaInputPackage.Build(new HaInput(SasUri, OutputUri));

        using ZipArchive archive = new(new MemoryStream(zip), ZipArchiveMode.Read);
        ZipArchiveEntry entry = Assert.Single(archive.Entries);
        Assert.Equal("manifest.json", entry.FullName);
        using JsonDocument manifest = JsonDocument.Parse(entry.Open());
        Assert.Equal(SasUri, manifest.RootElement.GetProperty("inventorySasUri").GetString());
        Assert.Equal(OutputUri, manifest.RootElement.GetProperty("assessmentSasUri").GetString());
    }

    [Fact]
    public async Task HaInputPackage_UploadsBlockBlobWithoutAuthorization()
    {
        var handler = new SequenceHandler(HttpStatusCode.Created);
        byte[] zip = HaInputPackage.Build(new HaInput(SasUri, OutputUri));

        var (status, error) = await HaInputPackage.UploadAsync(
            new HttpClient(handler), HaWriteSas, zip, NullLogger.Instance, (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.Equal(201, status);
        Assert.Null(error);
        HttpRequestMessage request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal(HaWriteSas, request.RequestUri!.ToString());
        Assert.Equal("BlockBlob", request.Headers.GetValues("x-ms-blob-type").Single());
        Assert.Null(request.Headers.Authorization);
        Assert.Equal(zip, handler.Bodies[0]);
    }

    [Fact]
    public async Task HaInputPackage_RetriesTransientErrors()
    {
        var handler = new SequenceHandler(HttpStatusCode.ServiceUnavailable, HttpStatusCode.TooManyRequests, HttpStatusCode.Created);
        var delays = new List<TimeSpan>();

        var (status, error) = await HaInputPackage.UploadAsync(
            new HttpClient(handler), HaWriteSas, [1], NullLogger.Instance,
            (delay, _) => { delays.Add(delay); return Task.CompletedTask; }, TestContext.Current.CancellationToken);

        Assert.Equal(201, status);
        Assert.Null(error);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal([TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)], delays);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, 1)]
    [InlineData(HttpStatusCode.InternalServerError, 3)]
    public async Task HaInputPackage_FailsAfterNonTransientErrorOrLastAttempt(HttpStatusCode failure, int expectedAttempts)
    {
        var handler = new SequenceHandler(failure);

        var (status, error) = await HaInputPackage.UploadAsync(
            new HttpClient(handler), HaWriteSas, [1], NullLogger.Instance, (_, _) => Task.CompletedTask, TestContext.Current.CancellationToken);

        Assert.Equal((int)failure, status);
        Assert.Contains($"HTTP {(int)failure}", error);
        Assert.DoesNotContain("sig=write", error);
        Assert.Equal(expectedAttempts, handler.Requests.Count);
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
        var client = Substitute.For<IAssessmentOperationStatusClient>();
        Guid id = Guid.NewGuid();
        client.GetOperationAsync(default, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(Running(id));
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";
        var command = new GetAssessmentStatusCommand(client, new HttpContextAccessor { HttpContext = context });

        var result = await command.RunAsync(new() { OperationId = id.ToString() }, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Null(result.AssessmentName);
        await client.Received(1).GetOperationAsync(id, null, "token", Arg.Any<CancellationToken>());
    }

    private ManualTimeProvider Time { get; } = new();

    private static AssessmentOperation Running(Guid id, int httpStatus = 200) => new(httpStatus, id, "Running", null, 5, null, null);

    private static CreateAssessmentOptions ValidOptions() =>
        new() { AssessmentName = "assess-1", TargetRegion = "East US", Currency = "usd", InventorySasUri = SasUri };

    private (CreateAssessmentCommand Command, ManualTimeProvider Time, IAssessmentCreateClient Create, IAssessmentOperationStatusClient Status, IHaSessionClient Ha) CreateCommand()
    {
        var create = Substitute.For<IAssessmentCreateClient>();
        var status = Substitute.For<IAssessmentOperationStatusClient>();
        var ha = Substitute.For<IHaSessionClient>();
        ha.StartAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new HaSessionResult(null, 200, "ha-1", false, "op-1", "req", null));
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";
        return (new CreateAssessmentCommand(create, status, ha, new HttpContextAccessor { HttpContext = context }), Time, create, status, ha);
    }

    private static Task<CreateAssessmentCommand.CreateAssessmentCommandResult> Run(CreateAssessmentCommand command, ManualTimeProvider time) =>
        command.RunAsync(ValidOptions(), (delay, _) => { time.Now += delay; return Task.CompletedTask; }, TestContext.Current.CancellationToken);
}
