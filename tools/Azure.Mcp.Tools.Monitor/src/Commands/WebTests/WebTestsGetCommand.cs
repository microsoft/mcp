// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

public sealed class WebTestsGetCommand(ILogger<WebTestsGetCommand> logger) : BaseMonitorWebTestsCommand<WebTestsGetOptions>
{
    private const string _commandTitle = "Get details of a specific web test";
    private const string _commandName = "get";

    private readonly Option<string> _webTestResourceNameOption = MonitorOptionDefinitions.WebTest.WebTestResourceName;

    public override string Name => _commandName;

    public override string Description =>
         $"""
        Gets details for a specific web test in the provided resource group based on webtest resource name.
        Returns detailed information about a single web test.
        """;

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    private readonly ILogger<WebTestsGetCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_webTestResourceNameOption);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    }

    protected override WebTestsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceName = parseResult.GetValueOrDefault(_webTestResourceNameOption);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var monitorWebTestService = context.GetService<IMonitorWebTestService>();
            var webTest = await monitorWebTestService.GetWebTest(
                options.Subscription!,
                options.ResourceGroup!,
                options.ResourceName!,
                options.Tenant,
                options.RetryPolicy);

            if (webTest != null)
            {
                context.Response.Results = ResponseResult.Create(
                    new WebTestsGetCommandResult(webTest),
                    MonitorJsonContext.Default.WebTestsGetCommandResult);
            }
            else
            {
                context.Response.Status = 404;
                context.Response.Message = $"Web test '{options.ResourceName}' not found in resource group '{options.ResourceGroup}'";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving web test '{Name}' in resource group '{ResourceGroup}', subscription '{Subscription}'",
                options.ResourceName, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WebTestsGetCommandResult(WebTestDetailedInfo WebTest);
}
