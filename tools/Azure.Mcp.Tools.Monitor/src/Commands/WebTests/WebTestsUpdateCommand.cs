// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Helpers;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.WebTests;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

public sealed class WebTestsUpdateCommand(ILogger<WebTestsUpdateCommand> logger) : BaseMonitorWebTestsCommand<WebTestsUpdateOptions>
{
    private const string CommandTitle = "Update a web test in Azure Monitor";

    private readonly ILogger<WebTestsUpdateCommand> _logger = logger;

    public override string Name => "update";

    public override string Description =>
        """
        Updates an existing standard web test in Azure Monitor. Ping/Multstep web tests are deprecated and are not supported.
        Returns the updated web test details.
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());

        // Add optional options - for update, most options become optional
        command.Options.Add(MonitorOptionDefinitions.WebTest.AppInsightsComponentId);
        command.Options.Add(MonitorOptionDefinitions.WebTest.ResourceLocation);
        command.Options.Add(MonitorOptionDefinitions.WebTest.Locations);
        command.Options.Add(MonitorOptionDefinitions.WebTest.RequestUrl);
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
            // Only validate explicitly provided values for update operations
            var locations = commandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.Locations.Name);
            if (locations != null && locations.Length == 0)
            {
                commandResult.AddError("If locations are specified, at least one location must be provided.");
            }

            var requestUrl = commandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.RequestUrl.Name);
            if (requestUrl != null && !Uri.TryCreate(requestUrl, UriKind.Absolute, out _))
            {
                commandResult.AddError("The request url option must be a valid absolute URL.");
            }

            var timeoutInSeconds = commandResult.GetValueWithoutDefault<int?>(MonitorOptionDefinitions.WebTest.TimeoutInSeconds.Name);
            if (timeoutInSeconds.HasValue && timeoutInSeconds > 120)
            {
                commandResult.AddError("The timeout cannot be greater than 2 minutes.");
            }
        });
    }

    protected override WebTestsUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        options.ResourceName = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.WebTest.WebTestResourceName.Name)!;
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.AppInsightsComponentId = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.AppInsightsComponentId.Name);
        options.Location = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.ResourceLocation.Name);
        options.Locations = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.Locations.Name);
        options.RequestUrl = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.RequestUrl.Name);
        options.WebTestName = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.WebTestName.Name);
        options.Description = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.Description.Name);
        options.Enabled = parseResult.CommandResult.GetValueWithoutDefault<bool?>(MonitorOptionDefinitions.WebTest.Enabled.Name);
        options.ExpectedStatusCode = parseResult.CommandResult.GetValueWithoutDefault<int?>(MonitorOptionDefinitions.WebTest.ExpectedStatusCode.Name);
        options.FollowRedirects = parseResult.CommandResult.GetValueWithoutDefault<bool?>(MonitorOptionDefinitions.WebTest.FollowRedirects.Name);
        options.FrequencyInSeconds = parseResult.CommandResult.GetValueWithoutDefault<int?>(MonitorOptionDefinitions.WebTest.FrequencyInSeconds.Name);
        options.Headers = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.Headers.Name);
        options.HttpVerb = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.HttpVerb.Name);
        options.IgnoreStatusCode = parseResult.CommandResult.GetValueWithoutDefault<bool?>(MonitorOptionDefinitions.WebTest.IgnoreStatusCode.Name);
        options.ParseRequests = parseResult.CommandResult.GetValueWithoutDefault<bool?>(MonitorOptionDefinitions.WebTest.ParseRequests.Name);
        options.RequestBody = parseResult.CommandResult.GetValueWithoutDefault<string>(MonitorOptionDefinitions.WebTest.RequestBody.Name);
        options.RetryEnabled = parseResult.CommandResult.GetValueWithoutDefault<bool?>(MonitorOptionDefinitions.WebTest.RetryEnabled.Name);
        options.SslCheck = parseResult.CommandResult.GetValueWithoutDefault<bool?>(MonitorOptionDefinitions.WebTest.SslCheck.Name);
        options.SslLifetimeCheckInDays = parseResult.CommandResult.GetValueWithoutDefault<int?>(MonitorOptionDefinitions.WebTest.SslLifetimeCheckInDays.Name);
        options.TimeoutInSeconds = parseResult.CommandResult.GetValueWithoutDefault<int?>(MonitorOptionDefinitions.WebTest.TimeoutInSeconds.Name);

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
            var locationsArray = options.Locations?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var headersDictionary = options.Headers == null ? null : OptionParsingHelpers.ParseKeyValuePairStringToDictionary(options.Headers);

            var monitorWebTestService = context.GetService<IMonitorWebTestService>();
            var webTest = await monitorWebTestService.UpdateWebTest(
                subscription: options.Subscription!,
                resourceGroup: options.ResourceGroup!,
                resourceName: options.ResourceName!,
                appInsightsComponentId: options.AppInsightsComponentId,
                location: options.Location,
                locations: locationsArray,
                requestUrl: options.RequestUrl,
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
                MonitorJsonContext.Default.WebTestsUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating web test '{WebTestName}' in resource group '{ResourceGroup}'",
                options.WebTestName ?? options.ResourceName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record WebTestsUpdateCommandResult(WebTestDetailedInfo WebTest);
}
