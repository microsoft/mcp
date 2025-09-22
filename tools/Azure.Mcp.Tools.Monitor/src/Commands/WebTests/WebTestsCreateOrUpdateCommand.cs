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

public sealed class WebTestsCreateOrUpdateCommand(ILogger<WebTestsCreateOrUpdateCommand> logger) : BaseMonitorWebTestsCommand<WebTestsCreateOrUpdateOptions>
{
    private const string _commandTitle = "Create or updates a web test in Azure Monitor";
    private const string _commandName = "createorupdate";

    private readonly ILogger<WebTestsCreateOrUpdateCommand> _logger = logger;

    private readonly Option<string> _resourceNameOption = MonitorOptionDefinitions.WebTest.WebTestResourceName;
    private readonly Option<string> _appInsightsComponentIdOption = MonitorOptionDefinitions.WebTest.AppInsightsComponentId;
    private readonly Option<string> _resourceLocationOption = MonitorOptionDefinitions.WebTest.ResourceLocation;
    private readonly Option<string> _locationsOption = MonitorOptionDefinitions.WebTest.Locations;
    private readonly Option<string> _requestUrlOption = MonitorOptionDefinitions.WebTest.RequestUrl;

    private readonly Option<string> _webTestNameOption = MonitorOptionDefinitions.WebTest.WebTestName;
    private readonly Option<string> _descriptionOption = MonitorOptionDefinitions.WebTest.Description;
    private readonly Option<bool> _enabledOption = MonitorOptionDefinitions.WebTest.Enabled;
    private readonly Option<int> _expectedStatusCodeOption = MonitorOptionDefinitions.WebTest.ExpectedStatusCode;
    private readonly Option<bool> _followRedirectsOption = MonitorOptionDefinitions.WebTest.FollowRedirects;
    private readonly Option<int> _frequencyInSecondsOption = MonitorOptionDefinitions.WebTest.FrequencyInSeconds;
    private readonly Option<string> _headersOption = MonitorOptionDefinitions.WebTest.Headers;
    private readonly Option<string> _httpVerbOption = MonitorOptionDefinitions.WebTest.HttpVerb;
    private readonly Option<bool> _ignoreStatusCodeOption = MonitorOptionDefinitions.WebTest.IgnoreStatusCode;
    private readonly Option<bool> _parseRequestsOption = MonitorOptionDefinitions.WebTest.ParseRequests;
    private readonly Option<string> _requestBodyOption = MonitorOptionDefinitions.WebTest.RequestBody;
    private readonly Option<bool> _retryEnabledOption = MonitorOptionDefinitions.WebTest.RetryEnabled;
    private readonly Option<bool> _sslCheckOption = MonitorOptionDefinitions.WebTest.SslCheck;
    private readonly Option<int> _sslLifetimeCheckInDaysOption = MonitorOptionDefinitions.WebTest.SslLifetimeCheckInDays;
    private readonly Option<int> _timeoutInSecondsOption = MonitorOptionDefinitions.WebTest.TimeoutInSeconds;

    public override string Name => _commandName;

    public override string Description =>
        """
        Creates a new standard web test in Azure Monitor or updates an existing one. Ping/Multstep web tests are deprecated and are not supported.
        Returns the created/updated web test details.
        """;

    public override string Title => _commandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add required options
        command.Options.Add(_resourceNameOption);
        command.Options.Add(_appInsightsComponentIdOption);
        command.Options.Add(_resourceLocationOption);
        command.Options.Add(_locationsOption);
        command.Options.Add(_requestUrlOption);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());

        // Add optional options
        command.Options.Add(_webTestNameOption);
        command.Options.Add(_descriptionOption);
        command.Options.Add(_enabledOption);
        command.Options.Add(_expectedStatusCodeOption);
        command.Options.Add(_followRedirectsOption);
        command.Options.Add(_frequencyInSecondsOption);
        command.Options.Add(_headersOption);
        command.Options.Add(_httpVerbOption);
        command.Options.Add(_ignoreStatusCodeOption);
        command.Options.Add(_parseRequestsOption);
        command.Options.Add(_requestBodyOption);
        command.Options.Add(_retryEnabledOption);
        command.Options.Add(_sslCheckOption);
        command.Options.Add(_sslLifetimeCheckInDaysOption);
        command.Options.Add(_timeoutInSecondsOption);
    }

    protected override WebTestsCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        options.ResourceName = parseResult.GetValueOrDefault(_resourceNameOption)!;
        options.AppInsightsComponentId = parseResult.GetValueOrDefault(_appInsightsComponentIdOption)!;
        options.Location = parseResult.GetValueOrDefault(_resourceLocationOption)!;
        options.Locations = parseResult.GetValueOrDefault(_locationsOption)!;
        options.RequestUrl = parseResult.GetValueOrDefault(_requestUrlOption)!;
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

        options.WebTestName = parseResult.GetValueOrDefault(_webTestNameOption);
        options.Description = parseResult.GetValueOrDefault(_descriptionOption);
        options.Enabled = parseResult.GetValueOrDefault(_enabledOption);
        options.ExpectedStatusCode = parseResult.GetValueOrDefault(_expectedStatusCodeOption);
        options.FollowRedirects = parseResult.GetValueOrDefault(_followRedirectsOption);
        options.FrequencyInSeconds = parseResult.GetValueOrDefault(_frequencyInSecondsOption);
        options.Headers = parseResult.GetValueOrDefault(_headersOption);
        options.HttpVerb = parseResult.GetValueOrDefault(_httpVerbOption);
        options.IgnoreStatusCode = parseResult.GetValueOrDefault(_ignoreStatusCodeOption);
        options.ParseRequests = parseResult.GetValueOrDefault(_parseRequestsOption);
        options.RequestBody = parseResult.GetValueOrDefault(_requestBodyOption);
        options.RetryEnabled = parseResult.GetValueOrDefault(_retryEnabledOption);
        options.SslCheck = parseResult.GetValueOrDefault(_sslCheckOption);
        options.SslLifetimeCheckInDays = parseResult.GetValueOrDefault(_sslLifetimeCheckInDaysOption);
        options.TimeoutInSeconds = parseResult.GetValueOrDefault(_timeoutInSecondsOption);

        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? response)
    {
        if (response == null)
        {
            return new ValidationResult { IsValid = false };
        }

        var baseValidation = base.Validate(commandResult, response);
        if (!baseValidation.IsValid)
        {
            return baseValidation;
        }

        var locations = commandResult.GetValueOrDefault(_locationsOption);
        if (locations == null || locations.Length == 0)
        {
            response.Status = 400;
            response.Message = "The locations option is required and must include at least one location.";
            return new ValidationResult { IsValid = false };
        }

        var requestUrl = commandResult.GetValueOrDefault(_requestUrlOption);
        if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out _))
        {
            response.Status = 400;
            response.Message = "The request url option must be a valid absolute URL.";
            return new ValidationResult { IsValid = false };
        }

        var timeoutInSeconds = commandResult.GetValueOrDefault(_timeoutInSecondsOption);
        if (timeoutInSeconds > 120)
        {
            response.Status = 400;
            response.Message = "The timeout cannot be greater than 2 minutes.";
            return new ValidationResult { IsValid = false };
        }

        return new ValidationResult { IsValid = true };
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

            var locationsArray = options.Locations!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var headersDictionary = options.Headers?
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x[0], x => x[1]) ?? new Dictionary<string, string>(0);

            var monitorWebTestService = context.GetService<IMonitorWebTestService>();
            var webTest = await monitorWebTestService.CreateOrUpdateWebTest(
                subscription: options.Subscription!,
                resourceGroup: options.ResourceGroup!,
                resourceName: options.ResourceName!,
                appInsightsComponentId: options.AppInsightsComponentId!,
                location: options.Location!,
                locations: locationsArray,
                requestUrl: options.RequestUrl!,
                webTestName: options.WebTestName,
                description: options.Description,
                enabled: options.Enabled,
                expectedStatusCode: options.ExpectedStatusCode,
                followRedirects: options.FollowRedirects,
                frequencyInSeconds: options.FrequencyInSeconds,
                headers: headersDictionary,
                httpVerb: options.HttpVerb,
                ignoreStatusCode: options.IgnoreStatusCode,
                parseRequests: options.ParseRequests,
                requestBody: options.RequestBody,
                retryEnabled: options.RetryEnabled,
                sslCheck: options.SslCheck,
                sslLifetimeCheckInDays: options.SslLifetimeCheckInDays,
                timeoutInSeconds: options.TimeoutInSeconds,
                tenant: options.Tenant,
                retryPolicy: options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new WebTestsCreateCommandResult(webTest),
                MonitorJsonContext.Default.WebTestsCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating web test '{WebTestName}' in resource group '{ResourceGroup}'",
                options.WebTestName ?? options.ResourceName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WebTestsCreateCommandResult(WebTestDetailedInfo WebTest);
}
