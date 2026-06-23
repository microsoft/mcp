// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppLens.Models;
using Azure.Mcp.Tools.AppLens.Options.Resource;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AppLens.Commands.Resource;

/// <summary>
/// Command to diagnose Azure resources using AppLens conversational diagnostics.
/// Subscription, resource group, and resource type are optional - the service uses
/// Azure Resource Graph to discover the resource by name when they are not provided.
/// </summary>
[CommandMetadata(
    Id = "92fb5b7d-f1d7-4834-a61a-e170ad8594ac",
    Name = "diagnose",
    Title = "Diagnose Azure Resource Issues",
    Description = "Get diagnostic help from App Lens for Azure application and service issues to identify what's wrong with a service. Ask questions about performance, slowness, failures, errors, application state, availability to receive expert analysis and solutions which can help when performing diagnostics and to address issues about performance and failures. Returns analysis, insights, and recommended solutions. Always use this tool before manually checking metrics or logs when users report performance or functionality issues. Only the resource name and question are required - subscription, resource group, and resource type are optional and used to narrow down results when multiple resources share the same name.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceDiagnoseCommand(ILogger<ResourceDiagnoseCommand> logger, IAppLensService appLensService)
    : BaseCommand<ResourceDiagnoseOptions, ResourceDiagnoseCommandResult>
{
    private readonly ILogger<ResourceDiagnoseCommand> _logger = logger;
    private readonly IAppLensService _appLensService = appLensService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ResourceDiagnoseOptions options, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Diagnosing resource. Question: {Question}, Resource: {Resource}, Options: {Options}",
                options.Question, options.Resource, options);

            var result = await _appLensService.DiagnoseResourceAsync(
                options.Question,
                options.Resource,
                options.Subscription,
                options.ResourceGroup,
                options.ResourceType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(result), AppLensJsonContext.Default.ResourceDiagnoseCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in diagnose. Exception: {Exception}", ex.Message);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
