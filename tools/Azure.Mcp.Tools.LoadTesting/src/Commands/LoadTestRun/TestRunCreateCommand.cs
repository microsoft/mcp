// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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
    public override string Name => "create";
    public override string Description =>
        $"""
        This will only create a test run for the selected test in the load test resource. It does not help in changing the test plan configuration. 
        This will NOT create a test and also NOT create a test resource. Only for the specified test, it will trigger/create a test run.
        Example:
        azmcp loadtesting testrun create --subscription <subscription> --resource-group <resource-group> --test-resource-name <test-resource-name> --test-id <test-id> --testrun-id <testrun-id> --display-name <display-name> --description <description> --old-testrun-id <old-testrun-id>
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.LoadTesting.TestRun);
        command.Options.Add(OptionDefinitions.LoadTesting.Test);
        command.Options.Add(OptionDefinitions.LoadTesting.DisplayName);
        command.Options.Add(OptionDefinitions.LoadTesting.Description);
        command.Options.Add(OptionDefinitions.LoadTesting.OldTestRunId);
    }

    protected override TestRunCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValueOrDefault<string>(OptionDefinitions.LoadTesting.TestRun.Name);
        options.TestId = parseResult.GetValueOrDefault<string>(OptionDefinitions.LoadTesting.Test.Name);
        options.DisplayName = parseResult.GetValueOrDefault<string>(OptionDefinitions.LoadTesting.DisplayName.Name);
        options.Description = parseResult.GetValueOrDefault<string>(OptionDefinitions.LoadTesting.Description.Name);
        options.OldTestRunId = parseResult.GetValueOrDefault<string>(OptionDefinitions.LoadTesting.OldTestRunId.Name);
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
