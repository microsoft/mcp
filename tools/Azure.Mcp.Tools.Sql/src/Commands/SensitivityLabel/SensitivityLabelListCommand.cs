// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.SensitivityLabel;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.SensitivityLabel;

[CommandMetadata(
    Id = "5f00b07f-5c24-4a14-bf0b-4d3702dfb1c4",
    Name = "list",
    Title = "List SQL Database Sensitivity Labels",
    Description = """
        Lists the current sensitivity labels (data classification) on the columns of an Azure SQL database.
        Use it to find which columns are labelled as personal, confidential or otherwise sensitive data,
        for example for GDPR or audit reviews. Returns one item per labelled column with its schema, table,
        column, label name, information type and rank. Reads classification metadata only, not table data.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SensitivityLabelListCommand(ISqlService sqlService, ILogger<SensitivityLabelListCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<SensitivityLabelListOptions, SensitivityLabelListCommand.SensitivityLabelListResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<SensitivityLabelListCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SensitivityLabelListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var labels = await _sqlService.ListSensitivityLabelsAsync(
                options.Server,
                options.Database,
                options.ResourceGroup,
                options.Subscription!,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(labels ?? []), SqlJsonContext.Default.SensitivityLabelListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL database sensitivity labels. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.Database, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL database or server not found. Verify the server name, database name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed reading the SQL database sensitivity labels. Verify you have appropriate permissions.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        // The base error payload carries the raw backend message even when GetErrorMessage is sanitized.
        if (ex is RequestFailedException { Status: (int)HttpStatusCode.Forbidden or (int)HttpStatusCode.NotFound })
        {
            context.Response.Results = null;
        }
    }

    public sealed record SensitivityLabelListResult(List<SqlDatabaseSensitivityLabel> SensitivityLabels);
}
