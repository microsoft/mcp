// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;

public sealed class TestRunCreateCommand(ILogger<TestRunCreateCommand> logger)
    : BaseLoadTestingCommand<TestRunCreateOptions>
{
    private const string _commandTitle = "Test Run Create";
    private readonly ILogger<TestRunCreateCommand> _logger = logger;
    private readonly Option<string> _testRunIdOption = OptionDefinitions.LoadTesting.TestRun;
    private readonly Option<string> _testIdOption = OptionDefinitions.LoadTesting.Test;
    private readonly Option<string> _displayNameOption = OptionDefinitions.LoadTesting.DisplayName;
    private readonly Option<string> _descriptionOption = OptionDefinitions.LoadTesting.Description;
    private readonly Option<string> _oldTestRunIdOption = OptionDefinitions.LoadTesting.OldTestRunId;
    public override string Name => "create";
    public override string Description =>
        $"""
        Executes a new load test run based on an existing test configuration under simulated user load. This command initiates the actual execution
        of a previously created test definition and provides real-time monitoring capabilities. A test run represents a single execution instance of your load test configuration. You can run
        the same test multiple times to validate performance improvements, compare results across different deployments, or establish performance baselines for your application.
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_testRunIdOption);
        command.Options.Add(_testIdOption);
        command.Options.Add(_displayNameOption);
        command.Options.Add(_descriptionOption);
        command.Options.Add(_oldTestRunIdOption);
    }

    protected override TestRunCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValue(_testRunIdOption);
        options.TestId = parseResult.GetValue(_testIdOption);
        options.DisplayName = parseResult.GetValue(_displayNameOption);
        options.Description = parseResult.GetValue(_descriptionOption);
        options.OldTestRunId = parseResult.GetValue(_oldTestRunIdOption);
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
            var results = await service.CreateOrUpdateLoadTestRunAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestId!,
                options.TestRunId,
                options.OldTestRunId,
                options.ResourceGroup,
                options.Tenant,
                options.DisplayName,
                options.Description,
                false, // DebugMode false will default to a normal test run - in future we may add a DebugMode option
                options.RetryPolicy);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestRunCreateCommandResult(results), LoadTestJsonContext.Default.TestRunCreateCommandResult) :
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
    internal record TestRunCreateCommandResult(TestRun TestRun);
}
