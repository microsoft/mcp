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
    Id = "0e3c8f2c-57ce-49c0-bff4-27c9573e7049",
    Name = "createorupdate",
    Title = "Test Run Create or Update",
    Description = """
        Create or update a load test run execution.
        Creates a new test run for a specified test in the load testing resource, or updates metadata and display properties of an existing test run.
        When creating: Triggers a new test run execution based on the existing test configuration. Use testrun ID to specify the new run identifier. Create operations are NOT idempotent - each call starts a new test run with unique timestamps and execution state.
        When updating: Modifies descriptive information (display name, description) of a completed or in-progress test run for better organization and documentation. Update operations are idempotent - repeated calls with same values produce the same result.
        This does not modify the test plan configuration or create a new test/resource - only manages test run executions.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class TestRunCreateOrUpdateCommand(ILogger<TestRunCreateOrUpdateCommand> logger, ILoadTestingService loadTestingService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TestRunCreateOrUpdateOptions, TestRunCreateOrUpdateCommand.TestRunCreateOrUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TestRunCreateOrUpdateCommand> _logger = logger;
    private readonly ILoadTestingService _loadTestingService = loadTestingService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TestRunCreateOrUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation(s)
            var results = await _loadTestingService.CreateOrUpdateLoadTestRunAsync(
                options.Subscription!,
                options.TestResourceName,
                options.TestId,
                options.TestrunId,
                options.OldTestrunId,
                options.ResourceGroup,
                options.Tenant,
                options.DisplayName,
                options.Description,
                false, // DebugMode false will default to a normal test run - in future we may add a DebugMode option
                options.RetryPolicy,
                cancellationToken);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestRunCreateOrUpdateCommandResult) :
                null;
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

    public sealed record TestRunCreateOrUpdateCommandResult(TestRun TestRun);
}
