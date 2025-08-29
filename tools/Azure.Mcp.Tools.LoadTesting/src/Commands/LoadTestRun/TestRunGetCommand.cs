// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;

public sealed class TestRunGetCommand(ILogger<TestRunGetCommand> logger)
    : BaseLoadTestingCommand<TestRunGetOptions>
{
    private const string _commandTitle = "Test Run Get";
    private readonly ILogger<TestRunGetCommand> _logger = logger;
    private readonly Option<string> _testRunIdOption = OptionDefinitions.LoadTesting.TestRun;
    public override string Name => "get";
    public override string Description =>
        $"""
        Retrieve details for a specific load test run (TestRun ID) in the specified Load Testing resource. Here the input is going to be the test run id and we get the details of that specific run only NOT the list of test runs.
            Returns run-level execution information: status, start/end times, progress, aggregated metrics, and available artifacts (logs/traces).
            Does NOT return the test definition/configuration; to inspect the test configuration use:
            azmcp loadtesting test get --test-id <test-id> --test-resource-name <resource> --resource-group <rg>
            Required parameters: --testrun-id and --test-resource-name (and --subscription). Use --resource-group when needed.
            Example:
            azmcp loadtesting testrun get --testrun-id <id> --test-resource-name <resource> --resource-group <rg> --subscription <sub>
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_testRunIdOption);
    }

    protected override TestRunGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValueForOption(_testRunIdOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            // Required validation step using the base Validate method
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }
            // Get the appropriate service from DI
            var service = context.GetService<ILoadTestingService>();
            // Call service operation(s)
            var results = await service.GetLoadTestRunAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestRunId!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestRunGetCommandResult(results), LoadTestJsonContext.Default.TestRunGetCommandResult) :
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
    internal record TestRunGetCommandResult(TestRun TestRun);
}
