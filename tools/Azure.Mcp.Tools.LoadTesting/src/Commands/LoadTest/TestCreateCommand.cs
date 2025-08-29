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
        Creates a new load test plan or configuration for performance testing scenarios. This command creates a basic URL-based load test that can be used to evaluate the performance
        and scalability of web applications and APIs. The test configuration defines the target endpoint, load parameters, and test duration. Once we create a test configuration plan, we can use that to trigger test runs to test the endpoints set using the 'azmcp loadtesting testrun create' command.
        This is not going to trigger or create any test runs and only will setup your configuration for your test plan. Also, this is NOT going to create any test resource in azure. 
        It will only create a test in an already existing load test resource.
        Example:
        azmcp loadtesting test create --test-id <test-id> --test-resource-name <test-resource-name> --duration 5 --virtual-users 50 --endpoint-url https://example.com --display-name "SmokeTest"
        Returns: the created test configuration (TestId and LoadTestConfiguration).
        """;
    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_loadTestIdOption);
        command.AddOption(_loadTestDescriptionOption);
        command.AddOption(_loadTestDisplayNameOption);
        command.AddOption(_loadTestEndpointOption);
        command.AddOption(_loadTestVirtualUsersOption);
        command.AddOption(_loadTestDurationOption);
        command.AddOption(_loadTestRampUpTimeOption);
    }

    protected override TestCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestId = parseResult.GetValueForOption(_loadTestIdOption);
        options.Description = parseResult.GetValueForOption(_loadTestDescriptionOption);
        options.DisplayName = parseResult.GetValueForOption(_loadTestDisplayNameOption);
        options.Endpoint = parseResult.GetValueForOption(_loadTestEndpointOption);
        options.VirtualUsers = parseResult.GetValueForOption(_loadTestVirtualUsersOption);
        options.Duration = parseResult.GetValueForOption(_loadTestDurationOption);
        options.RampUpTime = parseResult.GetValueForOption(_loadTestRampUpTimeOption);
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
