// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

public sealed class WebTestsGetCommand(ILogger<WebTestsGetCommand> logger) : BaseMonitorWebTestsCommand<WebTestsGetOptions>
{
    private const string CommandTitle = "Get or list web tests";

    public override string Id => "c9897ba5-445c-43dc-9902-e8454dbdc243";

    public override string Name => "get";

    public override string Description =>
         $"""
        Gets details for a specific web test or lists all web tests.
        When webtest-name is provided, returns detailed information about a single web test.
        When webtest-name is omitted, returns a list of all web tests in the subscription (optionally filtered by resource group).
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    private readonly ILogger<WebTestsGetCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MonitorOptionDefinitions.WebTest.WebTestResourceName.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());

        command.Validators.Add(commandResult =>
        {
            var webTestName = commandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.WebTestResourceName.Name);
            var resourceGroup = commandResult.GetValueWithoutDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

            if (!string.IsNullOrEmpty(webTestName) && string.IsNullOrEmpty(resourceGroup))
            {
                commandResult.AddError("The resource-group option is required when webtest-name is specified.");
            }
        });
    }

    protected override WebTestsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WebTestName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.WebTestResourceName.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var monitorWebTestService = context.GetService<IMonitorWebTestService>();

            // If webtest-name is provided, get a specific web test
            if (!string.IsNullOrEmpty(options.WebTestName))
            {
                var webTest = await monitorWebTestService.GetWebTest(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.WebTestName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (webTest != null)
                {
                    context.Response.Results = ResponseResult.Create(
                        new WebTestsGetCommandResult(webTest),
                        MonitorJsonContext.Default.WebTestsGetCommandResult);
                }
                else
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = $"Web test '{options.WebTestName}' not found in resource group '{options.ResourceGroup}'";
                }
            }
            else
            {
                // Otherwise, list web tests
                var webTests = options.ResourceGroup == null
                    ? await monitorWebTestService.ListWebTests(options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken)
                    : await monitorWebTestService.ListWebTests(options.Subscription!, options.ResourceGroup, options.Tenant, options.RetryPolicy, cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new WebTestsGetCommandListResult(webTests ?? []),
                    MonitorJsonContext.Default.WebTestsGetCommandListResult);
            }
        }
        catch (Exception ex)
        {
            var message = !string.IsNullOrEmpty(options.WebTestName)
                ? $"Error retrieving web test '{options.WebTestName}' in resource group '{options.ResourceGroup}', subscription '{options.Subscription}'"
                : $"Error listing web tests in subscription '{options.Subscription}'";
            _logger.LogError(ex, message);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WebTestsGetCommandResult(WebTestDetailedInfo WebTest);
    internal record WebTestsGetCommandListResult(List<WebTestSummaryInfo> WebTests);
}
