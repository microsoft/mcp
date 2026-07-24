// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;

[CommandMetadata(
    Id = "c39f6e9c-86a7-4cba-b267-0fa71f1ac743",
    Name = "create",
    Title = "Test Resource Create",
    Description = """
        Returns the created Load Testing resource. This creates the resource in Azure only. It does not create any test plan or test run. 
        Once the resource is setup, you can go and configure test plans in the resource and then trigger test runs for your test plans.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class TestResourceCreateCommand(ILogger<TestResourceCreateCommand> logger, ILoadTestingService loadTestingService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TestResourceCreateOptions, TestResourceCreateCommand.TestResourceCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TestResourceCreateCommand> _logger = logger;
    private readonly ILoadTestingService _loadTestingService = loadTestingService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TestResourceCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation(s)
            var results = await _loadTestingService.CreateOrUpdateLoadTestingResourceAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.TestResourceName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestResourceCreateCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with context information
            _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, TestResourceName: {TestResourceName}",
                Name, options.Subscription, options.ResourceGroup, options.TestResourceName);
            // Let base class handle standard error processing
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record TestResourceCreateCommandResult(TestResource LoadTest);
}
