// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Options;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTestRun;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTestRun;

public sealed class TestRunCreateOrUpdateCommand(ILogger<TestRunCreateOrUpdateCommand> logger)
    : BaseLoadTestingCommand<TestRunCreateOrUpdateOptions>
{
    private const string _commandTitle = "Test Run Create or Update";
    private readonly ILogger<TestRunCreateOrUpdateCommand> _logger = logger;
    public override string Id => "0e3c8f2c-57ce-49c0-bff4-27c9573e7049";
    public override string Name => "createorupdate";
    public override string Description =>
        $"""
        Create a test run or update test run display name and description in load testing resource.
        Create: Starts a new test run execution using testrun ID for a test in the load testing resource. Specify display name and description for the run.
        Update: Modifies test run display name and description for a testrun ID in the load testing resource.
        Manages test run executions in load testing resource - does not modify test configuration or resource settings.
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(LoadTestingOptionDefinitions.TestRun);
        command.Options.Add(LoadTestingOptionDefinitions.Test);
        command.Options.Add(LoadTestingOptionDefinitions.DisplayName.AsOptional());
        command.Options.Add(LoadTestingOptionDefinitions.Description.AsOptional());
        command.Options.Add(LoadTestingOptionDefinitions.OldTestRunId.AsOptional());
    }

    protected override TestRunCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestRunId = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.TestRun.Name);
        options.TestId = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.Test.Name);
        options.DisplayName = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.DisplayName.Name);
        options.Description = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.Description.Name);
        options.OldTestRunId = parseResult.GetValueOrDefault<string>(LoadTestingOptionDefinitions.OldTestRunId.Name);
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
                options.RetryPolicy,
                cancellationToken);
            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestRunCreateOrUpdateCommandResult) :
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
    internal record TestRunCreateOrUpdateCommandResult(TestRun TestRun);
}
