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
    private const string CommandTitle = "Create or updates a web test in Azure Monitor";

    private readonly ILogger<WebTestsCreateOrUpdateCommand> _logger = logger;

    public override string Name => "createorupdate";

    public override string Description =>
        """
        Creates a new standard web test in Azure Monitor or updates an existing one. Ping/Multstep web tests are deprecated and are not supported.
        Returns the created/updated web test details.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Add required options
        command.Options.Add(MonitorOptionDefinitions.WebTest.WebTestResourceName);
        command.Options.Add(MonitorOptionDefinitions.WebTest.AppInsightsComponentId);
        command.Options.Add(MonitorOptionDefinitions.WebTest.ResourceLocation);
        command.Options.Add(MonitorOptionDefinitions.WebTest.Locations);
        command.Options.Add(MonitorOptionDefinitions.WebTest.RequestUrl);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());

        // Add optional options
        command.Options.Add(MonitorOptionDefinitions.WebTest.WebTestName);
        command.Options.Add(MonitorOptionDefinitions.WebTest.Description);
        command.Options.Add(MonitorOptionDefinitions.WebTest.Enabled);
        command.Options.Add(MonitorOptionDefinitions.WebTest.ExpectedStatusCode);
        command.Options.Add(MonitorOptionDefinitions.WebTest.FollowRedirects);
        command.Options.Add(MonitorOptionDefinitions.WebTest.FrequencyInSeconds);
        command.Options.Add(MonitorOptionDefinitions.WebTest.Headers);
        command.Options.Add(MonitorOptionDefinitions.WebTest.HttpVerb);
        command.Options.Add(MonitorOptionDefinitions.WebTest.IgnoreStatusCode);
        command.Options.Add(MonitorOptionDefinitions.WebTest.ParseRequests);
        command.Options.Add(MonitorOptionDefinitions.WebTest.RequestBody);
        command.Options.Add(MonitorOptionDefinitions.WebTest.RetryEnabled);
        command.Options.Add(MonitorOptionDefinitions.WebTest.SslCheck);
        command.Options.Add(MonitorOptionDefinitions.WebTest.SslLifetimeCheckInDays);
        command.Options.Add(MonitorOptionDefinitions.WebTest.TimeoutInSeconds);
    }

    protected override WebTestsCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        options.ResourceName = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.WebTestResourceName)!;
        options.AppInsightsComponentId = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.AppInsightsComponentId)!;
        options.Location = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.ResourceLocation)!;
        options.Locations = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.Locations)!;
        options.RequestUrl = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.RequestUrl)!;
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

        options.WebTestName = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.WebTestName);
        options.Description = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.Description);
        options.Enabled = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.Enabled);
        options.ExpectedStatusCode = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.ExpectedStatusCode);
        options.FollowRedirects = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.FollowRedirects);
        options.FrequencyInSeconds = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.FrequencyInSeconds);
        options.Headers = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.Headers);
        options.HttpVerb = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.HttpVerb);
        options.IgnoreStatusCode = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.IgnoreStatusCode);
        options.ParseRequests = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.ParseRequests);
        options.RequestBody = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.RequestBody);
        options.RetryEnabled = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.RetryEnabled);
        options.SslCheck = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.SslCheck);
        options.SslLifetimeCheckInDays = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.SslLifetimeCheckInDays);
        options.TimeoutInSeconds = parseResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.TimeoutInSeconds);

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

        var locations = commandResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.Locations);
        if (locations == null || locations.Length == 0)
        {
            response.Status = 400;
            response.Message = "The locations option is required and must include at least one location.";
            return new ValidationResult { IsValid = false };
        }

        var requestUrl = commandResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.RequestUrl);
        if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out _))
        {
            response.Status = 400;
            response.Message = "The request url option must be a valid absolute URL.";
            return new ValidationResult { IsValid = false };
        }

        var timeoutInSeconds = commandResult.GetValueOrDefault(MonitorOptionDefinitions.WebTest.TimeoutInSeconds);
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
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
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
                new(webTest),
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
