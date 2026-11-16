// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureMigrate.Tests.Assessment;

public class AssessmentCommandTests
{
    private sealed class ManualTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
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
    [InlineData(null, null)]
    [InlineData("US", null)]
    [InlineData("USDD", null)]
    [InlineData("U5D", null)]
    public void NormalizeCurrency(string? currency, string? expected) =>
        Assert.Equal(expected, CreateAssessmentCommand.NormalizeCurrency(currency));

    [Theory]
    [InlineData(null, 2)]
    [InlineData(1, 2)]
    [InlineData(5, 5)]
    [InlineData(30, 10)]
    public void GetPollInterval_ClampsRetryAfter(int? retryAfter, int expectedSeconds) =>
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), CreateAssessmentCommand.GetPollInterval(retryAfter));

    [Fact]
    public async Task Create_WaitsForCompletionThenCreatesHa()
    {
        var (command, time, ha) = CreateCommand(new MockAssessmentServiceClient(Time));

        var result = await Run(command, time, TimeSpan.FromMinutes(3));

        Assert.True(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal("eastus", result.TargetRegion);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(201, result.HaHttpStatus);
        Assert.Contains("HA assessment has been created", result.Instructions);
        await ha.Received(1).PutHaAssessmentAsync(new("assess-1", "eastus", "USD"), "token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_TimesOut_ReturnsRunningAndSkipsHa()
    {
        var (command, time, ha) = CreateCommand(new MockAssessmentServiceClient(Time));

        var result = await Run(command, time, TimeSpan.FromSeconds(15));

        Assert.True(result.Success);
        Assert.False(result.IsCompleted);
        Assert.Equal("Running", result.Status);
        Assert.NotNull(result.OperationId);
        Assert.Null(result.HaHttpStatus);
        Assert.Contains("still running", result.Instructions);
        await ha.DidNotReceiveWithAnyArgs().PutHaAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_AssessmentFails_SkipsHa()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        Guid id = Guid.NewGuid();
        client.CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(202, id, "Running", null, 5, null, null));
        client.GetOperationAsync(default!, default, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new AssessmentOperation(200, id, "Failed", null, null, "Boom", "It failed."));
        var (command, time, ha) = CreateCommand(client);

        var result = await Run(command, time, TimeSpan.FromMinutes(3));

        Assert.True(result.IsCompleted);
        Assert.Equal("Failed", result.Status);
        Assert.Equal("It failed.", result.ErrorMessage);
        Assert.Contains("did not complete", result.Instructions);
        await ha.DidNotReceiveWithAnyArgs().PutHaAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Create_HaFails_ReturnsError()
    {
        var (command, time, ha) = CreateCommand(new MockAssessmentServiceClient(Time));
        ha.PutHaAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new HaAssessmentResult(500, null, "Internal", "HA down."));

        var result = await Run(command, time, TimeSpan.FromMinutes(3));

        Assert.False(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Equal(500, result.HaHttpStatus);
        Assert.Contains("HA assessment create failed", result.Error);
    }

    [Fact]
    public async Task Create_WithoutToken_FailsBeforeCallingService()
    {
        var client = Substitute.For<IAssessmentServiceClient>();
        var command = new CreateAssessmentCommand(client, Substitute.For<IHaAssessmentClient>(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        var result = await command.RunAsync(ValidOptions(), Time, (_, _) => Task.CompletedTask, TimeSpan.FromMinutes(3), TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Contains("No bearer token", result.Error);
        await client.DidNotReceiveWithAnyArgs().CreateAssessmentAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    private ManualTimeProvider Time { get; } = new();

    private static CreateAssessmentOptions ValidOptions() =>
        new() { AssessmentName = "assess-1", TargetRegion = "East US", Currency = "usd" };

    private (CreateAssessmentCommand Command, ManualTimeProvider Time, IHaAssessmentClient Ha) CreateCommand(IAssessmentServiceClient client)
    {
        var ha = Substitute.For<IHaAssessmentClient>();
        ha.PutHaAssessmentAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(new HaAssessmentResult(201, "Succeeded", null, null));
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer token";
        return (new CreateAssessmentCommand(client, ha, new HttpContextAccessor { HttpContext = context }), Time, ha);
    }

    private static Task<CreateAssessmentCommand.CreateAssessmentCommandResult> Run(
        CreateAssessmentCommand command, ManualTimeProvider time, TimeSpan timeout) =>
        command.RunAsync(ValidOptions(), time, (delay, _) => { time.Now += delay; return Task.CompletedTask; }, timeout, TestContext.Current.CancellationToken);

    [Fact]
    public async Task MockClient_ReportsRunningThenSucceeded()
    {
        var time = new ManualTimeProvider();
        var client = new MockAssessmentServiceClient(time);

        AssessmentOperation created = await client.CreateAssessmentAsync(new("assess-1", "eastus", "USD"), "token", TestContext.Current.CancellationToken);
        Assert.Equal(202, created.HttpStatus);
        Assert.Equal("Running", created.Status);
        Assert.NotNull(created.OperationId);
        Assert.Equal(
            $"https://assessment.mock.local/mcprestapi/assessments/assess-1/operationstatus/{created.OperationId:D}",
            created.TrackingUrl);

        time.Now += TimeSpan.FromSeconds(10);
        AssessmentOperation running = await client.GetOperationAsync("assess-1", created.OperationId!.Value, "token", TestContext.Current.CancellationToken);
        Assert.Equal("Running", running.Status);
        Assert.Equal(20, running.RetryAfterSeconds);

        time.Now += TimeSpan.FromSeconds(20);
        AssessmentOperation done = await client.GetOperationAsync("assess-1", created.OperationId.Value, "token", TestContext.Current.CancellationToken);
        Assert.Equal(200, done.HttpStatus);
        Assert.Equal("Succeeded", done.Status);
        Assert.Null(done.RetryAfterSeconds);
    }

    [Fact]
    public async Task MockClient_ReturnsNotFoundForUnknownOperationOrOtherName()
    {
        var client = new MockAssessmentServiceClient(new ManualTimeProvider());
        AssessmentOperation created = await client.CreateAssessmentAsync(new("assess-1", "eastus", "USD"), "token", TestContext.Current.CancellationToken);

        AssessmentOperation unknown = await client.GetOperationAsync("assess-1", Guid.NewGuid(), "token", TestContext.Current.CancellationToken);
        AssessmentOperation otherName = await client.GetOperationAsync("assess-2", created.OperationId!.Value, "token", TestContext.Current.CancellationToken);

        Assert.Equal(404, unknown.HttpStatus);
        Assert.Equal(404, otherName.HttpStatus);
        Assert.Equal("OperationNotFound", otherName.ErrorCode);
    }

    [Fact]
    public void ParseStatus_Running_IsNotCompletedWithRetry()
    {
        var result = GetAssessmentStatusCommand.Parse("a", "op", new(200, Guid.NewGuid(), "Running", null, 15, null, null));

        Assert.True(result.Success);
        Assert.False(result.IsCompleted);
        Assert.Equal(15, result.RetryAfterSeconds);
        Assert.Contains("still running", result.Instructions);
    }

    [Fact]
    public void ParseStatus_Succeeded_IsCompleted()
    {
        var result = GetAssessmentStatusCommand.Parse("a", "op", new(200, Guid.NewGuid(), "Succeeded", null, null, null, null));

        Assert.True(result.Success);
        Assert.True(result.IsCompleted);
        Assert.Null(result.RetryAfterSeconds);
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
}
