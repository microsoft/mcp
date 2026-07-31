// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Options.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

[CommandMetadata(
    Id = "c9897ba5-445c-43dc-9902-e8454dbdc243",
    Name = "get",
    Title = "Get or list web tests",
    Description = """
        Gets details for a specific web test or lists all web tests.
        When --webtest-resource is provided, returns detailed information about a single web test.
        When --webtest-resource is omitted, returns a list of all web tests in the subscription (optionally filtered by resource group).
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class WebTestsGetCommand(ILogger<WebTestsGetCommand> logger, IMonitorWebTestService monitorWebTestService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<WebTestsGetOptions, WebTestsGetCommand.WebTestsGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<WebTestsGetCommand> _logger = logger;
    private readonly IMonitorWebTestService _monitorWebTestService = monitorWebTestService;

    public override void ValidateOptions(WebTestsGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.WebtestResource) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            validationResult.Errors.Add("The --resource-group option is required when --webtest-resource is specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WebTestsGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If --webtest-resource is provided, get a specific web test
            if (!string.IsNullOrEmpty(options.WebtestResource))
            {
                var webTest = await _monitorWebTestService.GetWebTest(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.WebtestResource!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (webTest != null)
                {
                    context.Response.Results = ResponseResult.Create(new(webTest, null), MonitorJsonContext.Default.WebTestsGetCommandResult);
                }
                else
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = $"Web test '{options.WebtestResource}' not found in resource group '{options.ResourceGroup}'";
                }
            }
            else
            {
                // Otherwise, list web tests
                var webTests = options.ResourceGroup == null
                    ? await _monitorWebTestService.ListWebTests(options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken)
                    : await _monitorWebTestService.ListWebTests(options.Subscription!, options.ResourceGroup, options.Tenant, options.RetryPolicy, cancellationToken);

                context.Response.Results = ResponseResult.Create(new(null, webTests ?? []), MonitorJsonContext.Default.WebTestsGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            var message = !string.IsNullOrEmpty(options.WebtestResource)
                ? $"Error retrieving web test '{options.WebtestResource}' in resource group '{options.ResourceGroup}', subscription '{options.Subscription}'"
                : $"Error listing web tests in subscription '{options.Subscription}'";
            _logger.LogError(ex, message);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record WebTestsGetCommandResult(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] WebTestDetailedInfo? WebTest,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] List<WebTestSummaryInfo>? WebTests);
}
