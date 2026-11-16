// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;
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

    [Fact]
    public async Task MockClient_ReportsRunningThenSucceeded()
    {
        var time = new ManualTimeProvider();
        var client = new MockAssessmentServiceClient(time);

        AssessmentOperation created = await client.CreateAssessmentAsync(new("assess-1", "eastus", "USD"), "token", CancellationToken.None);
        Assert.Equal(202, created.HttpStatus);
        Assert.Equal("Running", created.Status);
        Assert.NotNull(created.OperationId);
        Assert.Equal(
            $"https://assessment.mock.local/mcprestapi/assessments/assess-1/operationstatus/{created.OperationId:D}",
            created.TrackingUrl);

        time.Now += TimeSpan.FromSeconds(10);
        AssessmentOperation running = await client.GetOperationAsync("assess-1", created.OperationId!.Value, "token", CancellationToken.None);
        Assert.Equal("Running", running.Status);
        Assert.Equal(20, running.RetryAfterSeconds);

        time.Now += TimeSpan.FromSeconds(20);
        AssessmentOperation done = await client.GetOperationAsync("assess-1", created.OperationId.Value, "token", CancellationToken.None);
        Assert.Equal(200, done.HttpStatus);
        Assert.Equal("Succeeded", done.Status);
        Assert.Null(done.RetryAfterSeconds);
    }

    [Fact]
    public async Task MockClient_ReturnsNotFoundForUnknownOperationOrOtherName()
    {
        var client = new MockAssessmentServiceClient(new ManualTimeProvider());
        AssessmentOperation created = await client.CreateAssessmentAsync(new("assess-1", "eastus", "USD"), "token", CancellationToken.None);

        AssessmentOperation unknown = await client.GetOperationAsync("assess-1", Guid.NewGuid(), "token", CancellationToken.None);
        AssessmentOperation otherName = await client.GetOperationAsync("assess-2", created.OperationId!.Value, "token", CancellationToken.None);

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
