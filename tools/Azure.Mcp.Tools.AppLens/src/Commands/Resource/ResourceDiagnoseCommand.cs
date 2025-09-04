// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
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
    : BaseCommand()
{
    private const string CommandTitle = "Diagnose Azure Resource Issues";
    private readonly ILogger<ResourceDiagnoseCommand> _logger = logger;

    private readonly Option<string> _questionOption = AppLensOptionDefinitions.Resource.Question;
    private readonly Option<string> _resourceNameOption = AppLensOptionDefinitions.Resource.ResourceName;
    private readonly Option<string?> _subscriptionNameOrIdOption = AppLensOptionDefinitions.Resource.SubscriptionNameOrId;
    private readonly Option<string?> _resourceGroupOption = AppLensOptionDefinitions.Resource.ResourceGroup;
    private readonly Option<string?> _resourceTypeOption = AppLensOptionDefinitions.Resource.ResourceType;

    public override string Name => "diagnose";

    public override string Description =>
        """
        This tool can be used to ask questions about application state using AppLens diagnostics, helping when doing diagnostics and addressing issues about performance and failures.

        This is able to investigate logs, telemetry and other performance sensors to provide insights and recommendations using AppLens conversational diagnostics.

        For example, the user may say: 'Why is my app slow?' or 'Please help me diagnose issues with my app' or 'Is my web site experiencing problems?'.

        Do _not_ ask for the 'subscriptionNameOrId', 'resourceGroup', or 'resourceType'; just pass null if they aren't known. If we can't find any resources matching 'resourceName', this returns an error message.

        Ask the user to check the spelling. If we find more than one resource matching 'resourceName', this returns the list of resources.

        Ask the user which one they are interested in, and then try again by filling in the 'subscriptionNameOrId' and 'resourceGroup'.

        If we find exactly one resource matching 'resourceName', this returns a list of insights and solutions to the user question.
        """;

    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_questionOption);
        command.AddOption(_resourceNameOption);
        command.AddOption(_subscriptionNameOrIdOption);
        command.AddOption(_resourceGroupOption);
        command.AddOption(_resourceTypeOption);
    }

    private ResourceDiagnoseOptions BindOptions(ParseResult parseResult)
    {
        return new ResourceDiagnoseOptions
        {
            Question = parseResult.GetValueForOption(_questionOption) ?? string.Empty,
            ResourceName = parseResult.GetValueForOption(_resourceNameOption) ?? string.Empty,
            SubscriptionNameOrId = parseResult.GetValueForOption(_subscriptionNameOrIdOption),
            ResourceGroup = parseResult.GetValueForOption(_resourceGroupOption),
            ResourceType = parseResult.GetValueForOption(_resourceTypeOption)
        };
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

            var service = context.GetService<IAppLensService>();

            _logger.LogInformation("Diagnosing resource. Question: {Question}, Resource: {ResourceName}, Options: {Options}",
                options.Question, options.ResourceName, options);

            var result = await service.DiagnoseResourceAsync(
                options.Question,
                options.ResourceName,
                options.SubscriptionNameOrId,
                options.ResourceGroup,
                options.ResourceType,
                options.RetryPolicy);

            var commandResult = new ResourceDiagnoseCommandResult(result);
            context.Response.Results = ResponseResult.Create(commandResult, AppLensJsonContext.Default.ResourceDiagnoseCommandResult);
            context.Response.Status = 200;
            context.Response.Message = "Successfully diagnosed resource using AppLens.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in diagnose. Question: {Question}, Resource: {ResourceName}, Options: {Options}",
                options.Question, options.ResourceName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
