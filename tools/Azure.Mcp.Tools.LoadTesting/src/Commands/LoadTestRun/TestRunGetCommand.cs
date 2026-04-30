// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;

[CommandMetadata(
    Id = "713313ec-b9a5-4a71-9953-5b2d4a7b5d7b",
    Name = "get",
    Title = "Test Run Get",
    Description = """
        Get load test run details by testrun ID, or list all test runs by test ID.
        Returns execution details including status, start/end times, progress, metrics, and artifacts.
        Does not return test configuration or resource details.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TestRunGetCommand(ILogger<TestRunGetCommand> logger, ILoadTestingService loadTestingService)
    : BaseLoadTestingCommand<TestRunGetOptions, CommandResponse>
{
    private readonly ILogger<TestRunGetCommand> _logger = logger;
    private readonly ILoadTestingService _loadTestingService = loadTestingService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TestRunGetOptions options, CancellationToken cancellationToken)
    {
        // options binding is implicit and convention based
        // pre-parse validation is also convention based
        // complex validation is moved to post-parse 👇

        if (string.IsNullOrEmpty(options.TestRunId) && string.IsNullOrEmpty(options.TestId))
        {
            throw new ArgumentException("Either --testrun or --test must be provided. Pass --testrun to get details about a specific run or pass --test to list all test runs for the test.");
        }
        else if (!string.IsNullOrEmpty(options.TestRunId) && !string.IsNullOrEmpty(options.TestId))
        {
            throw new ArgumentException("Cannot specify both --testrun and --test. Use one or the other.");
        }

        try
        {
            // If TestRunId is provided, get a single test run
            if (!string.IsNullOrEmpty(options.TestRunId))
            {
                var result = await _loadTestingService.GetLoadTestRunAsync(
                    options.Subscription!,
                    options.TestResourceName!,
                    options.TestRunId!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                // Set results if any were returned
                context.Response.Results = result != null
                    ? ResponseResult.Create(new([result]), LoadTestJsonContext.Default.TestRunGetCommandResult)
                    : null;
            }
            // Otherwise if TestId is provided, list all test runs for that test
            else if (!string.IsNullOrEmpty(options.TestId))
            {
                var results = await _loadTestingService.GetLoadTestRunsFromTestIdAsync(
                    options.Subscription!,
                    options.TestResourceName!,
                    options.TestId!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                context.Response.Results = ResponseResult.Create(new(results ?? []), LoadTestJsonContext.Default.TestRunGetCommandResult);
            }
            // If neither is provided, that's ok - validation will catch it
        }
        catch (Exception ex)
        {
            // Log error with context information
            _logger.LogError(ex, "Error in {Operation}. Options: {Options}", Name, options);
            // Let base class handle standard error processing
            HandleException(context, ex);
        }
        return context.Response;
    }
    internal record TestRunGetCommandResult(List<TestRun> TestRuns);
}
