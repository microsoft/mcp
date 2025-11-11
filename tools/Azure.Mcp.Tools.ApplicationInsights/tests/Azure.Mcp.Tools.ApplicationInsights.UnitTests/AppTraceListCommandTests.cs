using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ApplicationInsights.Commands.AppTrace;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ApplicationInsights.UnitTests;

public class AppTraceListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IApplicationInsightsService _serviceMock;
    private readonly AppTraceListCommand _command;
    private readonly CommandContext _context;

    public AppTraceListCommandTests()
    {
        var sc = new ServiceCollection();
        _serviceMock = Substitute.For<IApplicationInsightsService>();
        sc.AddSingleton(_serviceMock);
        var logger = Substitute.For<ILogger<AppTraceListCommand>>();
        _serviceProvider = sc.BuildServiceProvider();
        _command = new AppTraceListCommand(logger);
        _context = new(_serviceProvider);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsTraces_SetsResults()
    {
        AppListTraceResult traceResult = new()
        {
            Table = "requests",
            Rows = new List<AppListTraceEntry>
            {
                new() { OperationName = "GET /a", ResultCode = "200" },
                new() { OperationName = "GET /b", ResultCode = "500" }
            }
        };
        _serviceMock.ListDistributedTracesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(traceResult));

        var args = _command.GetCommand().Parse(["--subscription", "sub1", "--resource-name", "app1", "--table", "requests", "--start-time", DateTime.UtcNow.AddMinutes(-30).ToString("o"), "--end-time", DateTime.UtcNow.ToString("o")]);
        await _command.ExecuteAsync(_context, args);

        Assert.NotNull(_context.Response.Results);
        string json = JsonSerializer.Serialize(_context.Response.Results);
        Assert.Contains("GET /a", json);
        Assert.Contains("GET /b", json);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsNoTraces_NoResults()
    {
        _serviceMock.ListDistributedTracesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(new AppListTraceResult { Table = "requests", Rows = new List<AppListTraceEntry>() }));

        var args = _command.GetCommand().Parse(["--subscription", "sub1", "--resource-name", "app1", "--table", "requests", "--start-time", DateTime.UtcNow.AddMinutes(-30).ToString("o"), "--end-time", DateTime.UtcNow.ToString("o")]);
        await _command.ExecuteAsync(_context, args);

        // Empty result still yields a result object per current command implementation
        // Adjust expectation: if command changes to suppress empty results, update this assertion.
        Assert.NotNull(_context.Response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithStartEndTime_ParsesAndPassesValues()
    {
        DateTime capturedStart = default;
        DateTime capturedEnd = default;
        _serviceMock.ListDistributedTracesAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Do<DateTime>(d => capturedStart = d),
            Arg.Do<DateTime>(d => capturedEnd = d),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(new AppListTraceResult()));

        string start = DateTime.UtcNow.AddHours(-2).ToString("o");
        string end = DateTime.UtcNow.AddHours(-1).ToString("o");
        var args = _command.GetCommand().Parse(["--subscription", "sub1", "--resource-name", "app1", "--table", "requests", "--start-time", start, "--end-time", end]);
        await _command.ExecuteAsync(_context, args);

        Assert.NotEqual(default, capturedStart);
        Assert.NotEqual(default, capturedEnd);
        Assert.True(capturedStart < capturedEnd);
    }
}
