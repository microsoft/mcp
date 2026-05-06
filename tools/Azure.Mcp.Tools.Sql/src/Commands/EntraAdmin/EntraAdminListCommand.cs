// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.EntraAdmin;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.EntraAdmin;

public sealed class EntraAdminListCommand(ISqlService sqlService, ILogger<EntraAdminListCommand> logger)
    : BaseSqlCommand<EntraAdminListOptions, EntraAdminListCommand.EntraAdminListResult>(logger)
{
    protected override JsonTypeInfo<EntraAdminListResult> ResultTypeInfo => SqlJsonContext.Default.EntraAdminListResult;
    private readonly ISqlService _sqlService = sqlService;
    private const string CommandTitle = "List SQL Server Entra ID Administrators";

    public override string Id => "240aac03-0eb0-4cd3-91f8-475577289186";

    public override string Name => "list";

    public override string Description =>
        """
        Gets a list of Microsoft Entra ID administrators for a SQL server. This command retrieves all
        Entra ID administrators configured for the specified SQL server, including their display names, object IDs,
        and tenant information. Returns an array of Entra ID administrator objects with their properties.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var administrators = await _sqlService.GetEntraAdministratorsAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            SetResult(context, new(administrators ?? []));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL server Entra ID administrators. Server: {Server}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record EntraAdminListResult(List<SqlServerEntraAdministrator> Administrators);
}
