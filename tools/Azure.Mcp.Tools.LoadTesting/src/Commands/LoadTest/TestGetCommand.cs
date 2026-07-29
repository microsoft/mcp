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
    Id = "be7c3864-0713-42f8-8eb7-b7ca28a951fb",
    Name = "get",
    Title = "Test Get",
    Description = """
        Get the configuration and setup details for a load test by its test ID in a Load Testing resource.
        Returns only the test definition, including duration, ramp-up, virtual users, and endpoint. Does not return any test run results or execution data. Also does NOT return and resource details. Only the test configuration is fetched.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TestGetCommand(ILogger<TestGetCommand> logger, ILoadTestingService loadTestingService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TestGetOptions, TestGetCommand.TestGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TestGetCommand> _logger = logger;
    private readonly ILoadTestingService _loadTestingService = loadTestingService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TestGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation(s)
            var results = await _loadTestingService.GetTestAsync(
                options.Subscription!,
                options.TestResourceName,
                options.TestId,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestGetCommandResult) :
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

    public sealed record TestGetCommandResult(Test Test);
}
