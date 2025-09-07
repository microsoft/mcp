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
        Retrieves comprehensive details and status information for a specific load test run execution.
        This command provides real-time insights into test performance metrics, execution timeline,
        and final results to help you analyze your application's behavior under load.
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_testRunIdOption);
    }

    protected override TestRunGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValue(_testRunIdOption);
        return options;
    }

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
