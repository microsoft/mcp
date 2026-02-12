// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Options;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;

public sealed class TestRunGetCommand(ILogger<TestRunGetCommand> logger)
    : BaseLoadTestingCommand<TestRunGetOptions>
{
    private const string _commandTitle = "Test Run Get";
    private readonly ILogger<TestRunGetCommand> _logger = logger;
    public override string Id => "713313ec-b9a5-4a71-9953-5b2d4a7b5d7b";
    public override string Name => "get";
    public override string Description =>
        $"""
        Get load test run details in a load test resource.
        With testrun ID: Returns a single test run with execution details including status, start/end times, progress, metrics, and artifacts in the load test resource.
        With test ID: Returns all load test runs for a specific test in the load test resource.
        Returns test run execution data only - not test configuration or resource details.
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(LoadTestingOptionDefinitions.TestRun.AsOptional());
        command.Options.Add(LoadTestingOptionDefinitions.Test.AsOptional());

        command.Validators.Add(commandResult =>
        {
            var testRunId = commandResult.GetValueWithoutDefault<string>(LoadTestingOptionDefinitions.TestRun.Name);
            var testId = commandResult.GetValueWithoutDefault<string>(LoadTestingOptionDefinitions.Test.Name);

            if (string.IsNullOrEmpty(testRunId) && string.IsNullOrEmpty(testId))
            {
                commandResult.AddError("Either --testrun or --test must be provided.");
            }
        });
    }

    protected override TestRunGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.TestRun.Name);
        options.TestId = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.Test.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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
            
            // If TestRunId is provided, get a single test run
            if (!string.IsNullOrEmpty(options.TestRunId))
            {
                var result = await service.GetLoadTestRunAsync(
                    options.Subscription!,
                    options.TestResourceName!,
                    options.TestRunId!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                // Set results if any were returned
                context.Response.Results = result != null ?
                    ResponseResult.Create(new TestRunGetCommandResult([result]), LoadTestJsonContext.Default.TestRunGetCommandResult) :
                    null;
            }
            // Otherwise if TestId is provided, list all test runs for that test
            else if (!string.IsNullOrEmpty(options.TestId))
            {
                var results = await service.GetLoadTestRunsFromTestIdAsync(
                    options.Subscription!,
                    options.TestResourceName!,
                    options.TestId!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
                context.Response.Results = ResponseResult.Create(new TestRunGetCommandResult(results ?? []), LoadTestJsonContext.Default.TestRunGetCommandResult);
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
