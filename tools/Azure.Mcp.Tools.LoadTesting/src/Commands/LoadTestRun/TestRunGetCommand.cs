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

public sealed class TestRunGetCommand(ILogger<TestRunGetCommand> logger)
    : BaseLoadTestingCommand<TestRunGetOptions>
{
    private const string _commandTitle = "Test Run Get";
    private readonly ILogger<TestRunGetCommand> _logger = logger;
    public override string Name => "get";
    public override string Description =>
        $"""
        Get details for a specific test run by testrun ID.
        Use this to retrieve a single run's execution details (not a list). Returns status, start/end times, progress, aggregated metrics, and available artifacts (logs/traces). 
        Does NOT return the test plan/configuration or the test resource. Only the test run details. Also it is used to get details of SINGLE testrun based on its id. For a list of runs use testrun list command instead.
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
        command.Options.Add(OptionDefinitions.LoadTesting.TestRun);
    }

    protected override TestRunGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValueOrDefault<string>(OptionDefinitions.LoadTesting.TestRun.Name);
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
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestRunGetCommandResult) :
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
