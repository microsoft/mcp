// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
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

    public override string Name => "get";
    public override string Description =>
        $"""
        Get the Azure Load Testing test configuration for the specified load test id in the specified load test resource.
        This command retrieves the details of a specific load test configuration, including its parameters and settings. Based on this we can see what all parameters were set for the test configuration.
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
        command.Options.Add(OptionDefinitions.LoadTesting.Test);
    }

    protected override TestGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TestId = parseResult.GetValueOrDefault<string>(OptionDefinitions.LoadTesting.Test.Name);
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
            var results = await service.GetTestAsync(
                options.Subscription!,
                options.TestResourceName!,
                options.TestId!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy);

            // Set results if any were returned
            context.Response.Results = results != null ?
                ResponseResult.Create(new(results), LoadTestJsonContext.Default.TestGetCommandResult) :
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
