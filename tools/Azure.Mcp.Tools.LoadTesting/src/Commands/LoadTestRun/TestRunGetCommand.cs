// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
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
public sealed class TestRunGetCommand(ILogger<TestRunGetCommand> logger, ILoadTestingService loadTestingService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TestRunGetOptions, TestRunGetCommand.TestRunGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TestRunGetCommand> _logger = logger;
    private readonly ILoadTestingService _loadTestingService = loadTestingService;

    public override void ValidateOptions(TestRunGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.TestrunId) && string.IsNullOrEmpty(options.TestId))
        {
            validationResult.Errors.Add("Either --testrun-id or --test-id must be provided. Pass --testrun-id to get details about a specific run or pass --test-id to list all test runs for the test.");
        }
        else if (!string.IsNullOrEmpty(options.TestrunId) && !string.IsNullOrEmpty(options.TestId))
        {
            validationResult.Errors.Add("Cannot specify both --testrun-id and --test-id. Use one or the other.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TestRunGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If TestRunId is provided, get a single test run
            if (!string.IsNullOrEmpty(options.TestrunId))
            {
                var result = await _loadTestingService.GetLoadTestRunAsync(
                    options.Subscription!,
                    options.TestResourceName,
                    options.TestrunId,
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
                    options.TestResourceName,
                    options.TestId,
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
            _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}, TestResourceName: {TestResourceName}, TestId: {TestId}, TestrunId: {TestrunId}",
                Name, options.Subscription, options.TestResourceName, options.TestId, options.TestrunId);
            // Let base class handle standard error processing
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record TestRunGetCommandResult(List<TestRun> TestRuns);
}
