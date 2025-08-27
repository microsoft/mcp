// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;

public sealed class TestRunUpdateCommand(ILogger<TestRunUpdateCommand> logger)
    : BaseLoadTestingCommand<TestRunUpdateOptions>
{
    private const string _commandTitle = "Test Run Update";
    private readonly ILogger<TestRunUpdateCommand> _logger = logger;
    private readonly Option<string> _testRunIdOption = OptionDefinitions.LoadTesting.TestRun;
    private readonly Option<string> _testIdOption = OptionDefinitions.LoadTesting.Test;
    private readonly Option<string> _displayNameOption = OptionDefinitions.LoadTesting.DisplayName;
    private readonly Option<string> _descriptionOption = OptionDefinitions.LoadTesting.Description;
    public override string Name => "update";
    public override string Description =>
        $"""
        Updates the metadata and display properties of a completed or in-progress load test run execution.
        This command allows you to modify descriptive information for better organization, documentation,
        and identification of test runs without affecting the actual test execution or results.
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
    }

    protected override TestRunUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValue(_testRunIdOption);
        options.TestId = parseResult.GetValue(_testIdOption);
        options.DisplayName = parseResult.GetValue(_displayNameOption);
        options.Description = parseResult.GetValue(_descriptionOption);
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
                oldTestRunId: null,
                options.ResourceGroup,
                options.Tenant,
                options.DisplayName,
                options.Description,
                false, // DebugMode false will default to a normal test run - in future we may add a DebugMode option
                options.RetryPolicy);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestRunUpdateCommandResult(results), LoadTestJsonContext.Default.TestRunUpdateCommandResult) :
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
    internal record TestRunUpdateCommandResult(TestRun TestRun);
}
