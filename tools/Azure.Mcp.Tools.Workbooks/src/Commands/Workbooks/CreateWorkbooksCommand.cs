// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

[CommandMetadata(
    Id = "a49c650d-8568-4b63-8bad-35eb6d9ab0a7",
    Name = "create",
    Title = "Create Workbook",
    Description = """
        Create a new workbook in the specified resource group and subscription.
        You can set the display name and serialized data JSON content for the workbook.
        Returns the created workbook information upon successful completion.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CreateWorkbooksCommand(ILogger<CreateWorkbooksCommand> logger, IWorkbooksService workbooksService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CreateWorkbookOptions, CreateWorkbooksCommand.CreateWorkbooksCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CreateWorkbooksCommand> _logger = logger;
    private readonly IWorkbooksService _workbooksService = workbooksService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CreateWorkbookOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var createdWorkbook = await _workbooksService.CreateWorkbookAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.DisplayName,
                options.SerializedContent,
                /*
                 * The source ID is optional, defaulting to "azure monitor" if not provided.
                 * "azure monitor" is the default for workbooks created in the Azure Monitor extension,
                 * otherwise the workbook will display an error when opening.
                 */
                options.SourceId ?? "azure monitor",
                options.RetryPolicy,
                options.Tenant,
                cancellationToken) ?? throw new InvalidOperationException("Failed to create workbook");

            context.Response.Results = ResponseResult.Create(new(createdWorkbook), WorkbooksJsonContext.Default.CreateWorkbooksCommandResult);
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
