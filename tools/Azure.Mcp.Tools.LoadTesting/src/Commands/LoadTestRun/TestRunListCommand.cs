// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;

public sealed class TestRunListCommand(ILogger<TestRunListCommand> logger)
    : BaseLoadTestingCommand<TestRunListOptions>
{
    private const string _commandTitle = "Test Run List";
    private readonly ILogger<TestRunListCommand> _logger = logger;
    private readonly Option<string> _loadTestIdOption = OptionDefinitions.LoadTesting.Test;
    public override string Name => "list";
    public override string Description =>
        $"""
        List all test runs for a given test in a Load Testing resource.
        Use this to retrieve the list of runs created for a specific test. Returns run-level metadata for each run (run id, display name, status, start/end times, duration, brief metrics summary). 
        Does NOT return the test. To get full details for a single run use azmcp loadtesting testrun get
        Example:
        azmcp loadtesting testrun list --test-id <test-id> --test-resource-name <resource> --resource-group <rg> --subscription <sub>
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_loadTestIdOption);
    }

    protected override TestRunListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestId = parseResult.GetValue(_loadTestIdOption);
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
            var results = await service.GetLoadTestRunsFromTestIdAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestId!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestRunListCommandResult(results), LoadTestJsonContext.Default.TestRunListCommandResult) :
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
    internal record TestRunListCommandResult(List<TestRun> TestRun);
}
