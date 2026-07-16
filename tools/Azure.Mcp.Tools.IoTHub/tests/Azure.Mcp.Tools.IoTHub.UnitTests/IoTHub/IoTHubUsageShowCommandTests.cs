// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Commands.IoTHub;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.IoTHub;

public class IoTHubUsageShowCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIoTHubService _service;
    private readonly ILogger<IoTHubUsageShowCommand> _logger;
    private readonly IoTHubUsageShowCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public IoTHubUsageShowCommandTests()
    {
        _service = Substitute.For<IIoTHubService>();
        _logger = Substitute.For<ILogger<IoTHubUsageShowCommand>>();

        var collection = new ServiceCollection().AddSingleton(_service);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new IoTHubUsageShowCommand(_service, _logger);
        _context = new CommandContext(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsUsageSnapshot()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        var expected = new IoTHubUsageSnapshot(
            HubName: name,
            SnapshotTime: DateTimeOffset.UtcNow,
            StartTime: DateTimeOffset.UtcNow.AddDays(-1),
            EndTime: DateTimeOffset.UtcNow,
            ConnectedDeviceCount: new IoTHubDeviceCountStats(0, 0, 0),
            TotalDeviceCount: new IoTHubDeviceCountStats(243, 243, 240.98),
            DailyMessageQuotaUsed: 31,
            DailyMessageQuotaUsedByDay: null,
            TotalMessagesUsed: null,
            D2CMessageCount: 0,
            ThrottlingErrors: 0,
            PeakHourlyThrottlingErrors: 0,
            Sku: "S1",
            Units: 1,
            RecommendedSku: null);

        _service.GetUsageSnapshot(
            name,
            resourceGroup,
            subscription,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _service.Received(1).GetUsageSnapshot(
            name,
            resourceGroup,
            subscription,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--resource-group test-rg --subscription sub-id")] // missing --name
    [InlineData("--name test-hub --subscription sub-id")] // missing --resource-group
    [InlineData("--name test-hub --resource-group test-rg")] // missing --subscription
    public async Task ExecuteAsync_MissingRequiredOption_ReturnsValidationError(string argsString)
    {
        // Arrange
        var args = _commandDefinition.Parse(argsString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);

        await _service.DidNotReceive().GetUsageSnapshot(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesError()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";

        _service.GetUsageSnapshot(
            name,
            resourceGroup,
            subscription,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns<IoTHubUsageSnapshot>(_ => throw new InvalidOperationException("boom"));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsStartAndEndTime()
    {
        // Arrange
        var name = "test-hub";
        var resourceGroup = "test-rg";
        var subscription = "sub-id";
        var startTime = "2026-07-07T00:00:00Z";
        var endTime = "2026-07-08T00:00:00Z";

        _service.GetUsageSnapshot(
            name,
            resourceGroup,
            subscription,
            startTime,
            endTime,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(new IoTHubUsageSnapshot(
                name,
                DateTimeOffset.UtcNow,
                DateTimeOffset.Parse(startTime),
                DateTimeOffset.Parse(endTime),
                new IoTHubDeviceCountStats(0, 0, 0),
                new IoTHubDeviceCountStats(1, 1, 1),
                1, null, null, 0, 0, 0, "S1", 1, null));

        var args = _commandDefinition.Parse([
            "--name", name,
            "--resource-group", resourceGroup,
            "--subscription", subscription,
            "--start-time", startTime,
            "--end-time", endTime
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await _service.Received(1).GetUsageSnapshot(
            name,
            resourceGroup,
            subscription,
            startTime,
            endTime,
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("F1", "S1")]
    [InlineData("S1", "S2")]
    [InlineData("S2", "S3")]
    [InlineData("s2", "S3")] // case-insensitive
    public void DetermineRecommendedSku_AboveThreshold_RecommendsNextTier(string sku, string expected)
    {
        Assert.Equal(expected, IoTHubService.DetermineRecommendedSku(1001, sku));
    }

    [Fact]
    public void DetermineRecommendedSku_S3_NeverRecommends()
    {
        // S3 is the top Standard tier, so no upgrade is ever recommended.
        Assert.Null(IoTHubService.DetermineRecommendedSku(50000, "S3"));
    }

    [Theory]
    [InlineData(1000d)] // exactly at the threshold is not above it
    [InlineData(0d)]
    [InlineData(null)]
    public void DetermineRecommendedSku_AtOrBelowThreshold_ReturnsNull(double? peak)
    {
        Assert.Null(IoTHubService.DetermineRecommendedSku(peak, "S1"));
    }

    [Fact]
    public void DetermineRecommendedSku_SubHourBurstAboveThreshold_StillRecommends()
    {
        // A partial-hour bucket with more than 1000 throttling errors still triggers a recommendation.
        Assert.Equal("S2", IoTHubService.DetermineRecommendedSku(1500, "S1"));
    }

    [Theory]
    [InlineData("B1")]
    [InlineData("B2")]
    [InlineData("B3")]
    public void DetermineRecommendedSku_BasicTier_ReturnsNull(string sku)
    {
        // Basic tiers have no defined single upgrade target here, so no recommendation is made.
        Assert.Null(IoTHubService.DetermineRecommendedSku(5000, sku));
    }
}
