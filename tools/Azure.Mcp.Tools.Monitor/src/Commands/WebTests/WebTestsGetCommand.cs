// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
    private const string CommandTitle = "Get details of a specific web test";

    public override string Name => "get";

    public override string Description =>
         $"""
        Gets details for a specific web test in the provided resource group based on webtest resource name.
        Returns detailed information about a single web test.
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
        command.Options.Add(MonitorOptionDefinitions.WebTest.WebTestResourceName);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    }

    protected override WebTestsGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.WebTestResourceName.Name);
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
                context.Response.Status = HttpStatusCode.NotFound;
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
