// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;

[CommandMetadata(
    Id = "2153384b-02ea-47b3-a069-7f5f9a709d66",
    Name = "create",
    Title = "Test Create",
    Description = """
        Creates a new load test plan or configuration for performance testing scenarios. This command creates a basic URL-based load test that can be used to evaluate the performance
        and scalability of web applications and APIs. The test configuration defines target endpoint, load parameters, and test duration. Once we create a test plan, we can use that to trigger test runs to test the endpoints set using the 'azmcp loadtesting testrun create' command.
        This is NOT going to trigger or create any test runs and only will setup your test plan. Also, this is NOT going to create any test resource in azure. 
        It will only create a test in an already existing load test resource.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class TestCreateCommand(ILogger<TestCreateCommand> logger, ILoadTestingService loadTestingService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TestCreateOptions, TestCreateCommand.TestCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TestCreateCommand> _logger = logger;
    private readonly ILoadTestingService _loadTestingService = loadTestingService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TestCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation(s)
            var results = await _loadTestingService.CreateTestAsync(
                options.Subscription!,
                options.TestResourceName,
                options.TestId,
                options.ResourceGroup,
                options.DisplayName,
                options.Description,
                options.Duration,
                options.VirtualUsers,
                options.RampUpTime,
                options.Endpoint,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestCreateCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with context information
            _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}, TestResourceName: {TestResourceName}, TestId: {TestId}",
                Name, options.Subscription, options.TestResourceName, options.TestId);
            // Let base class handle standard error processing
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record TestCreateCommandResult(Test Test);
}
