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
    Id = "eb44ef6c-93dc-4fa1-949c-a5e8939d5052",
    Name = "list",
    Title = "Test Resource List",
    Description = """
        Lists all Azure Load Testing resources available in the selected subscription and resource group.
        Returns metadata for each resource, including name, location, and status. Use this to discover, manage, or audit load testing resources in your environment. Does not return test plans or test runs.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TestResourceListCommand(ILogger<TestResourceListCommand> logger, ILoadTestingService loadTestingService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TestResourceListOptions, TestResourceListCommand.TestResourceListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TestResourceListCommand> _logger = logger;
    private readonly ILoadTestingService _loadTestingService = loadTestingService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TestResourceListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation(s)
            var results = await _loadTestingService.GetLoadTestResourcesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.TestResourceName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);
            // Set results if any were returned
            context.Response.Results = ResponseResult.Create(new(results ?? []), LoadTestJsonContext.Default.TestResourceListCommandResult);
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

    public sealed record TestResourceListCommandResult(List<TestResource> LoadTest);
}
