// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

public sealed class WebTestsCreateCommand(ILogger<WebTestsCreateCommand> logger) : BaseMonitorWebTestsCommand<WebTestsCreateOptions>
{
    private const string CommandTitle = "Create a web test in Azure Monitor";

    private readonly ILogger<WebTestsCreateCommand> _logger = logger;

    public override string Name => "create";

    public override string Description =>
        """
        Creates a new standard web test in Azure Monitor. Ping/Multstep web tests are deprecated and are not supported.
        Returns the created web test details.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
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
        command.Options.Add(MonitorOptionDefinitions.WebTest.AppInsightsComponentId.AsRequired());
        command.Options.Add(MonitorOptionDefinitions.WebTest.ResourceLocation.AsRequired());
        command.Options.Add(MonitorOptionDefinitions.WebTest.Locations.AsRequired());
        command.Options.Add(MonitorOptionDefinitions.WebTest.RequestUrl.AsRequired());
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

        command.Validators.Add(commandResult =>
        {
            var locations = commandResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.Locations.Name);
            if (locations == null || locations.Length == 0)
            {
                commandResult.AddError("The locations option is required and must include at least one location.");
            }

            var requestUrl = commandResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.RequestUrl.Name);
            if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out _))
            {
                commandResult.AddError("The request url option must be a valid absolute URL.");
            }

            var timeoutInSeconds = commandResult.GetValueOrDefault<int?>(MonitorOptionDefinitions.WebTest.TimeoutInSeconds.Name);
            if (timeoutInSeconds.HasValue && timeoutInSeconds.Value > 120)
            {
                commandResult.AddError("The timeout cannot be greater than 2 minutes.");
            }
        });
    }

    protected override WebTestsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        options.ResourceName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.WebTestResourceName.Name)!;
        options.AppInsightsComponentId = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.AppInsightsComponentId.Name)!;
        options.Location = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.ResourceLocation.Name)!;
        options.Locations = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.Locations.Name)!;
        options.RequestUrl = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.RequestUrl.Name)!;
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

        options.WebTestName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.WebTestName.Name);
        options.Description = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.Description.Name);
        options.Enabled = parseResult.GetValueOrDefault<bool?>(MonitorOptionDefinitions.WebTest.Enabled.Name);
        options.ExpectedStatusCode = parseResult.GetValueOrDefault<int?>(MonitorOptionDefinitions.WebTest.ExpectedStatusCode.Name);
        options.FollowRedirects = parseResult.GetValueOrDefault<bool?>(MonitorOptionDefinitions.WebTest.FollowRedirects.Name);
        options.FrequencyInSeconds = parseResult.GetValueOrDefault<int?>(MonitorOptionDefinitions.WebTest.FrequencyInSeconds.Name);
        options.Headers = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.Headers.Name);
        options.HttpVerb = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.HttpVerb.Name);
        options.IgnoreStatusCode = parseResult.GetValueOrDefault<bool?>(MonitorOptionDefinitions.WebTest.IgnoreStatusCode.Name);
        options.ParseRequests = parseResult.GetValueOrDefault<bool?>(MonitorOptionDefinitions.WebTest.ParseRequests.Name);
        options.RequestBody = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.RequestBody.Name);
        options.RetryEnabled = parseResult.GetValueOrDefault<bool?>(MonitorOptionDefinitions.WebTest.RetryEnabled.Name);
        options.SslCheck = parseResult.GetValueOrDefault<bool?>(MonitorOptionDefinitions.WebTest.SslCheck.Name);
        options.SslLifetimeCheckInDays = parseResult.GetValueOrDefault<int?>(MonitorOptionDefinitions.WebTest.SslLifetimeCheckInDays.Name);
        options.TimeoutInSeconds = parseResult.GetValueOrDefault<int?>(MonitorOptionDefinitions.WebTest.TimeoutInSeconds.Name);

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
            var locationsArray = options.Locations!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var headersDictionary = options.Headers == null ? new Dictionary<string, string>(0) : OptionParsingHelpers.ParseKeyValuePairStringToDictionary(options.Headers);

            var monitorWebTestService = context.GetService<IMonitorWebTestService>();
            var webTest = await monitorWebTestService.CreateWebTest(
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
