// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Options.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

public sealed class WebTestsListCommand(ILogger<WebTestsListCommand> logger) : BaseMonitorWebTestsCommand<WebTestsListOptions>
{
    private const string CommandTitle = "List all web tests in a subscription or resource group";

    private readonly ILogger<WebTestsListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
         $"""
        Lists all web tests in a specified subscription and optionally, a resource group.
        Returns a list of web tests.
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override WebTestsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var monitorWebTestService = context.GetService<IMonitorWebTestService>();
            var webTests = options.ResourceGroup == null
                ? await monitorWebTestService.ListWebTests(options.Subscription!, options.Tenant, options.RetryPolicy)
                : await monitorWebTestService.ListWebTests(options.Subscription!, options.ResourceGroup, options.Tenant, options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(webTests ?? []), MonitorJsonContext.Default.WebTestsListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error listing web tests in subscription '{options.Subscription}'");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WebTestsListCommandResult(List<WebTestSummaryInfo> WebTests);
}
