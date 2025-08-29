// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;

public sealed class TestGetCommand(ILogger<TestGetCommand> logger)
    : BaseLoadTestingCommand<TestGetOptions>
{
    private const string _commandTitle = "Test Get";
    private readonly ILogger<TestGetCommand> _logger = logger;
    private readonly Option<string> _loadTestIdOption = OptionDefinitions.LoadTesting.Test;

    public override string Name => "get";
    public override string Description =>
        $"""
        Retrieve the configuration for a load test (Test ID) defined in a Load Testing resource.
        Returns only the test definition and configuration (duration, ramp-up, virtual users, endpoint, etc.).
        Does NOT return any test runs or run-level data. To get run details (status, metrics, artifacts), use:
        azmcp loadtesting testrun get --testrun-id <id> --test-resource-name <resource> --resource-group <rg>
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_loadTestIdOption);
    }

    protected override TestGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestId = parseResult.GetValueForOption(_loadTestIdOption);
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
            var results = await service.GetTestAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestId!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestGetCommandResult(results), LoadTestJsonContext.Default.TestGetCommandResult) :
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
    internal record TestGetCommandResult(Test Test);
}
