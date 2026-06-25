// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Options.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

[CommandMetadata(
    Id = "aa5a22bc-6a04-4bc0-a963-b6e462b5cdc4",
    Name = "createorupdate",
    Title = "Create or update a web test in Azure Monitor",
    Description = """
        Create or update a standard web test in Azure Monitor to monitor endpoint availability.
        Use this to set up new web tests or modify existing ones with monitoring configurations like URL, frequency, locations, and expected responses.
        Automatically creates a new test if it doesn't exist, or updates an existing test with new settings.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class WebTestsCreateOrUpdateCommand(ILogger<WebTestsCreateOrUpdateCommand> logger, IMonitorWebTestService monitorWebTestService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<WebTestsCreateOrUpdateOptions, WebTestsCreateOrUpdateCommand.WebTestsCreateOrUpdateCommandResult>(subscriptionResolver)
{

    private readonly ILogger<WebTestsCreateOrUpdateCommand> _logger = logger;
    private readonly IMonitorWebTestService _monitorWebTestService = monitorWebTestService;

    public override void ValidateOptions(WebTestsCreateOrUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.WebtestLocations != null && options.WebtestLocations.Length == 0)
        {
            validationResult.Errors.Add("If webtest-locations are specified, at least one location must be provided.");
        }

        if (options.RequestUrl != null && !Uri.TryCreate(options.RequestUrl, UriKind.Absolute, out _))
        {
            validationResult.Errors.Add("The request-url option must be a valid absolute URL.");
        }

        if (options.Timeout.HasValue && options.Timeout > 120)
        {
            validationResult.Errors.Add("The timeout cannot be greater than 2 minutes.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, WebTestsCreateOrUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Check if the web test exists
            WebTestDetailedInfo? existingWebTest = null;
            try
            {
                existingWebTest = await _monitorWebTestService.GetWebTest(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.WebtestResource,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);
            }
            catch
            {
                // Web test doesn't exist, will create
            }

            WebTestDetailedInfo webTest;

            if (existingWebTest == null)
            {
                // Create new web test - validate required parameters
                if (string.IsNullOrEmpty(options.AppinsightsComponent))
                {
                    throw new ArgumentException("The appinsights-component option is required when creating a new web test.");
                }
                if (string.IsNullOrEmpty(options.Location))
                {
                    throw new ArgumentException("The location option is required when creating a new web test.");
                }
                if (string.IsNullOrEmpty(options.WebtestLocations))
                {
                    throw new ArgumentException("The webtest-locations option is required when creating a new web test.");
                }
                if (string.IsNullOrEmpty(options.RequestUrl))
                {
                    throw new ArgumentException("The request-url option is required when creating a new web test.");
                }

                var locationsArray = options.WebtestLocations!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var headersDictionary = options.Headers == null ? new Dictionary<string, string>(0) : OptionParsingHelpers.ParseKeyValuePairStringToDictionary(options.Headers);

                webTest = await _monitorWebTestService.CreateWebTest(
                    subscription: options.Subscription!,
                    resourceGroup: options.ResourceGroup,
                    resourceName: options.WebtestResource,
                    appInsightsComponentId: options.AppinsightsComponent,
                    location: options.Location,
                    locations: locationsArray,
                    requestUrl: options.RequestUrl,
                    webTestName: options.Webtest,
                    description: options.Description,
                    enabled: options.Enabled,
                    expectedStatusCode: options.ExpectedStatusCode,
                    followRedirects: options.FollowRedirects,
                    frequencyInSeconds: options.Frequency,
                    headers: headersDictionary,
                    httpVerb: options.HttpVerb,
                    ignoreStatusCode: options.IgnoreStatusCode,
                    parseRequests: options.ParseRequests,
                    requestBody: options.RequestBody,
                    retryEnabled: options.RetryEnabled,
                    sslCheck: options.SslCheck,
                    sslLifetimeCheckInDays: options.SslLifetimeCheck,
                    timeoutInSeconds: options.Timeout,
                    tenant: options.Tenant,
                    retryPolicy: options.RetryPolicy,
                    cancellationToken: cancellationToken);
            }
            else
            {
                // Update existing web test
                var locationsArray = options.WebtestLocations?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var headersDictionary = options.Headers == null ? null : OptionParsingHelpers.ParseKeyValuePairStringToDictionary(options.Headers);

                webTest = await _monitorWebTestService.UpdateWebTest(
                    subscription: options.Subscription!,
                    resourceGroup: options.ResourceGroup,
                    resourceName: options.WebtestResource,
                    appInsightsComponentId: options.AppinsightsComponent,
                    location: options.Location,
                    locations: locationsArray,
                    requestUrl: options.RequestUrl,
                    webTestName: options.Webtest,
                    description: options.Description,
                    enabled: options.Enabled,
                    expectedStatusCode: options.ExpectedStatusCode,
                    followRedirects: options.FollowRedirects,
                    frequencyInSeconds: options.Frequency,
                    headers: headersDictionary,
                    httpVerb: options.HttpVerb,
                    ignoreStatusCode: options.IgnoreStatusCode,
                    parseRequests: options.ParseRequests,
                    requestBody: options.RequestBody,
                    retryEnabled: options.RetryEnabled,
                    sslCheck: options.SslCheck,
                    sslLifetimeCheckInDays: options.SslLifetimeCheck,
                    timeoutInSeconds: options.Timeout,
                    tenant: options.Tenant,
                    retryPolicy: options.RetryPolicy,
                    cancellationToken: cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(
                new(webTest),
                MonitorJsonContext.Default.WebTestsCreateOrUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or updating web test '{WebTestName}' in resource group '{ResourceGroup}'",
                options.Webtest ?? options.WebtestResource, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record WebTestsCreateOrUpdateCommandResult(WebTestDetailedInfo WebTest);
}
