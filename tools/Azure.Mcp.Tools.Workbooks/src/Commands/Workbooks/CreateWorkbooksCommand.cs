// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

public sealed class CreateWorkbooksCommand(ILogger<CreateWorkbooksCommand> logger) : SubscriptionCommand<CreateWorkbookOptions>
{
    private const string CommandTitle = "Create Workbook";
    private readonly ILogger<CreateWorkbooksCommand> _logger = logger;

    private readonly Option<string> _displayNameOption = WorkbooksOptionDefinitions.DisplayNameRequired;
    private readonly Option<string> _serializedContentOption = WorkbooksOptionDefinitions.SerializedContentRequired;
    private readonly Option<string> _sourceIdOption = WorkbooksOptionDefinitions.SourceId;

    public override string Name => "create";

    public override string Description =>
        """
        Create a new workbook in the specified resource group and subscription.
        You can set the display name and serialized data JSON content for the workbook.
        Returns the created workbook information upon successful completion.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.AddOption(_displayNameOption);
        command.AddOption(_serializedContentOption);
        command.AddOption(_sourceIdOption);
    }

    protected override CreateWorkbookOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DisplayName = parseResult.GetValueForOption(_displayNameOption);
        options.SerializedContent = parseResult.GetValueForOption(_serializedContentOption);
        options.SourceId = parseResult.GetValueForOption(_sourceIdOption);
        return options;
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

            var workbooksService = context.GetService<IWorkbooksService>();
            var createdWorkbook = await workbooksService.CreateWorkbook(
                options.Subscription!,
                options.ResourceGroup!,
                options.DisplayName!,
                options.SerializedContent!,
                /**
                 * The source ID is optional, defaulting to "azure monitor" if not provided.
                 * "azure monitor" is the default for workbooks created in the Azure Monitor extension,
                 * otherwise the workbook will display an error when opening.
                 */
                options.SourceId ?? "azure monitor",
                options.RetryPolicy,
                options.Tenant) ?? throw new InvalidOperationException("Failed to create workbook");

            context.Response.Results = ResponseResult.Create(
                new CreateWorkbooksCommandResult(createdWorkbook),
                WorkbooksJsonContext.Default.CreateWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workbook '{DisplayName}' in resource group '{ResourceGroup}'", options.DisplayName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CreateWorkbooksCommandResult(WorkbookInfo Workbook);
}
