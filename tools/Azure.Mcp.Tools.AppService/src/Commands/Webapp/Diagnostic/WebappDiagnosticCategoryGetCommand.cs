// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Tools.AppService.Options.Webapp.Diagnostic;
using Azure.Mcp.Tools.AppService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppService.Commands.Webapp.Diagnostic;

public sealed class WebappDiagnosticCategoryGetCommand(ILogger<WebappDiagnosticCategoryGetCommand> logger)
    : BaseWebappDiagnosticCommand<WebappDiagnosticCategoryGetOptions>(diagnosticCategoryRequired: false)
{
    private const string CommandTitle = "Gets the Diagnostic Category detail for an Azure App Service Web App";
    private readonly ILogger<WebappDiagnosticCategoryGetCommand> _logger = logger;
    public override string Id => "036b569a-d576-4b19-9ae4-63937ad0fbe7";
    public override string Name => "get-category";

    public override string Description =>
        """
        Retrieves detailed information about diagnostic categories for an Azure App Service web app, returning the name,
        kind, type, and description for each category. If a diagnostic category is provided the details for that
        specific category is returned. Otherwise all diagnostic categories are returned.
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

    protected override void RegisterOptions(Command command) => base.RegisterOptions(command);

    protected override WebappDiagnosticCategoryGetOptions BindOptions(ParseResult parseResult) => base.BindOptions(parseResult);

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
            var diagnosticCategories = await appServiceService.GetWebAppDiagnosticCategoriesAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AppName!,
                options.DiagnosticCategory,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(diagnosticCategories), AppServiceJsonContext.Default.WebappDiagnosticCategoryGetResult);
        }
        catch (Exception ex)
        {
            if (options.DiagnosticCategory == null)
            {
                _logger.LogError(ex, "Failed to get diagnostic categories for Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.AppName, options.Subscription, options.ResourceGroup);
            }
            else
            {
                _logger.LogError(ex, "Failed to get the diagnostic category '{DiagnosticCategory}' for Web App '{AppName}' in subscription {Subscription} and resource group {ResourceGroup}",
                    options.DiagnosticCategory, options.AppName, options.Subscription, options.ResourceGroup);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record WebappDiagnosticCategoryGetResult(List<WebappDiagnosticCategoryDetails> WebappDiagnosticCategories);
}
