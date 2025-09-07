// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.EntraAdmin;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.EntraAdmin;

public sealed class EntraAdminListCommand(ILogger<EntraAdminListCommand> logger)
    : BaseSqlCommand<EntraAdminListOptions>(logger)
{
    private const string CommandTitle = "List SQL Server Entra ID Administrators";

    public override string Name => "list";

    public override string Description =>
        """
        Gets a list of Microsoft Entra ID administrators for a SQL server. This command retrieves all
        Entra ID administrators configured for the specified SQL server, including their display names, object IDs,
        and tenant information. Returns an array of Entra ID administrator objects with their properties.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var sqlService = context.GetService<ISqlService>();

            var administrators = await sqlService.GetEntraAdministratorsAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = administrators?.Count > 0
                ? ResponseResult.Create(
                    new EntraAdminListResult(administrators),
                    SqlJsonContext.Default.EntraAdminListResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL server Entra ID administrators. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record EntraAdminListResult(List<SqlServerEntraAdministrator> Administrators);
}
