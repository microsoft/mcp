// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestResource;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestResource;

public sealed class TestResourceCreateCommand(ILogger<TestResourceCreateCommand> logger)
    : BaseLoadTestingCommand<TestResourceCreateOptions>
{
    private const string _commandTitle = "Test Resource Create";
    private readonly ILogger<TestResourceCreateCommand> _logger = logger;
    public override string Name => "create";
    public override string Description =>
        $"""
        Returns the created Load Testing resource. This creates the resource in Azure only. It does not create any test plan or test run. 
        Once the resource is setup, you can go and configure test plans in the resource and then trigger test runs for your test plans.
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Get the appropriate service from DI
            var service = context.GetService<ILoadTestingService>();

            // Call service operation(s)
            var results = await service.CreateOrUpdateLoadTestingResourceAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.TestResourceName!,
                options.Tenant,
                options.RetryPolicy);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestResourceCreateCommandResult) :
                null;
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
    internal record TestResourceCreateCommandResult(TestResource LoadTest);
}
