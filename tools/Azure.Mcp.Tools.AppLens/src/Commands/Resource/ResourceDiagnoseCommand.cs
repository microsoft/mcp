// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AppLens.Models;
using Azure.Mcp.Tools.AppLens.Options;
using Azure.Mcp.Tools.AppLens.Options.Resource;
using Azure.Mcp.Tools.AppLens.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppLens.Commands.Resource;

/// <summary>
/// Command to diagnose Azure resources using AppLens conversational diagnostics.
/// </summary>
public sealed class ResourceDiagnoseCommand(ILogger<ResourceDiagnoseCommand> logger)
    : SubscriptionCommand<ResourceDiagnoseOptions>
{
    private const string CommandTitle = "Diagnose Azure Resource Issues";
    private readonly ILogger<ResourceDiagnoseCommand> _logger = logger;

    private readonly Option<string> _questionOption = AppLensOptionDefinitions.Question;
    private readonly Option<string> _resourceOption = AppLensOptionDefinitions.Resource;
    private readonly Option<string?> _resourceTypeOption = AppLensOptionDefinitions.ResourceType;

    public override string Name => "diagnose";

    public override string Description =>
        """
        **PRIMARY USE: Diagnose Azure resource performance issues, slowness, failures, and availability problems.**

        Always use this tool BEFORE manually checking metrics or logs when users report performance or functionality issues.

        Use the Azure CLI tool to find the 'subscription', 'resourceGroup', and 'resourceType' parameters before asking user to provide that information."
        This tool can be used to ask questions about application state, this tool can help when doing diagnostics and address issues about performance and failures.

        If you get a resourceId, parse it to get the 'subscription', 'resourceGroup', and 'resourceType' parameters of the resource. ResourceIds are in the format:
        /subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{resource}

        Once proper input is provided this tool returns a list of insights and solutions to the user question.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_questionOption);
        command.Options.Add(_resourceOption);
        command.Options.Add(_resourceTypeOption);
        RequireResourceGroup();
    }

    protected override ResourceDiagnoseOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Question = parseResult.GetValueOrDefault(_questionOption) ?? string.Empty;
        options.Resource = parseResult.GetValueOrDefault(_resourceOption) ?? string.Empty;
        options.ResourceType = parseResult.GetValueOrDefault(_resourceTypeOption) ?? string.Empty;
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            ResourceDiagnoseOptions options = BindOptions(parseResult);

            var service = context.GetService<IAppLensService>();

            _logger.LogInformation("Diagnosing resource. Question: {Question}, Resource: {Resource}, Options: {Options}",
                options.Question, options.Resource, options);

            var result = await service.DiagnoseResourceAsync(
                options.Question,
                options.Resource,
                options.Subscription!,
                options.ResourceGroup,
                options.ResourceType,
                options.Tenant);

            var commandResult = new ResourceDiagnoseCommandResult(result);
            context.Response.Results = ResponseResult.Create(commandResult, AppLensJsonContext.Default.ResourceDiagnoseCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in diagnose. Exception: {Exception}", ex.Message);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
