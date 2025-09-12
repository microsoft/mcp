// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Options.LoadTest;
using Azure.Mcp.Tools.LoadTesting.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.LoadTesting.Commands.LoadTest;

public sealed class TestCreateCommand(ILogger<TestCreateCommand> logger)
    : BaseLoadTestingCommand<TestCreateOptions>
{
    private const string _commandTitle = "Test Create";
    private readonly ILogger<TestCreateCommand> _logger = logger;
    private readonly Option<string> _loadTestIdOption = OptionDefinitions.LoadTesting.Test;
    private readonly Option<string> _loadTestDescriptionOption = OptionDefinitions.LoadTesting.Description;
    private readonly Option<string> _loadTestDisplayNameOption = OptionDefinitions.LoadTesting.DisplayName;
    private readonly Option<string> _loadTestEndpointOption = OptionDefinitions.LoadTesting.Endpoint;
    private readonly Option<int> _loadTestVirtualUsersOption = OptionDefinitions.LoadTesting.VirtualUsers;
    private readonly Option<int> _loadTestDurationOption = OptionDefinitions.LoadTesting.Duration;
    private readonly Option<int> _loadTestRampUpTimeOption = OptionDefinitions.LoadTesting.RampUpTime;
    public override string Name => "create";
    public override string Description =>
        $"""
        Creates a new Azure Load Testing test configuration for performance testing scenarios. This command creates a basic URL-based load test that can be used to evaluate the performance
        and scalability of web applications and APIs. The test configuration defines the target endpoint, load parameters, and test duration. Once we create a test configuration plan, we can use that to trigger test runs to test the endpoints set.
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
        command.Options.Add(_loadTestIdOption);
        command.Options.Add(_loadTestDescriptionOption);
        command.Options.Add(_loadTestDisplayNameOption);
        command.Options.Add(_loadTestEndpointOption);
        command.Options.Add(_loadTestVirtualUsersOption);
        command.Options.Add(_loadTestDurationOption);
        command.Options.Add(_loadTestRampUpTimeOption);
    }

    protected override TestCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestId = parseResult.GetValue(_loadTestIdOption);
        options.Description = parseResult.GetValue(_loadTestDescriptionOption);
        options.DisplayName = parseResult.GetValue(_loadTestDisplayNameOption);
        options.Endpoint = parseResult.GetValue(_loadTestEndpointOption);
        options.VirtualUsers = parseResult.GetValue(_loadTestVirtualUsersOption);
        options.Duration = parseResult.GetValue(_loadTestDurationOption);
        options.RampUpTime = parseResult.GetValue(_loadTestRampUpTimeOption);
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
            var results = await service.CreateTestAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestId!,
                options.ResourceGroup,
                options.DisplayName,
                options.Description,
                options.Duration,
                options.VirtualUsers,
                options.RampUpTime,
                options.Endpoint,
                options.Tenant,
                options.RetryPolicy);

            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new TestCreateCommandResult(results), LoadTestJsonContext.Default.TestCreateCommandResult) :
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
    internal record TestCreateCommandResult(Test Test);
}
