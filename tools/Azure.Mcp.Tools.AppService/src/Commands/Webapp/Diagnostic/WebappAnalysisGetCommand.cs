// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options;
using Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

public sealed class WebappAnalysisGetCommand(ILogger<WebappAnalysisGetCommand> logger)
    : BaseWebappDiagnosticCommand<WebappAnalysisGetOptions>(diagnosticCategoryRequired: true)
{
    private const string CommandTitle = "Gets the Site Analysis for a Diagnostic Category";
    private readonly ILogger<WebappAnalysisGetCommand> _logger = logger;
    public override string Id => "c82ca7b9-a8b4-4343-915d-21e5c8508229";
    public override string Name => "get-analysis";

    public override string Description =>
        """
        Retrieves detailed information about site analysis for the specified diagnostic category, returning the name,
        kind, type, and description for each analysis. Optionally, an analysis name can be provided to retrieve a
        single analysis; otherwise, all analyses for the diagnostic category are returned.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AppServiceOptionDefinitions.AnalysisName);
    }

    protected override WebappAnalysisGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.AnalysisName = parseResult.GetValueOrDefault<string>(AppServiceOptionDefinitions.AnalysisName.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        // Validate first, then bind
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            context.Activity?.AddTag("subscription", options.Subscription);

            var appServiceService = context.GetService<IAppServiceService>();
            var analyses = await appServiceService.GetWebAppAnalysesAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.DiagnosticCategory!,
                options.AnalysisName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(analyses), AppServiceJsonContext.Default.WebappAnalysisGetResult);
        }
        catch (Exception ex)
        {
            if (options.AnalysisName == null)
            {
                _logger.LogError(ex, "Failed to get diagnostic category analyses for diagnostic category '{DiagnosticCategory}' and Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.DiagnosticCategory, options.AppName, options.Subscription, options.ResourceGroup);
            }
            else
            {
                _logger.LogError(ex, "Failed to get diagnostic category analysis '{Analysis}' for diagnostic category '{DiagnosticCategory}' and Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.AnalysisName, options.DiagnosticCategory, options.AppName, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record WebappAnalysisGetResult(List<WebappAnalysisDetails> WebappAnalyses);
}
