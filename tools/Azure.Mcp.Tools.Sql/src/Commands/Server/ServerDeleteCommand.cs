// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

public sealed class ServerDeleteCommand(ILogger<ServerDeleteCommand> logger)
    : BaseSqlCommand<ServerDeleteOptions>(logger)
{
    private const string CommandTitle = "Delete SQL Server";

    public override string Name => "delete";

    public override string Description =>
        """
        Deletes an Azure SQL server and all of its databases from the specified resource group.
        This operation is irreversible and will permanently remove the server and all its data.
        Use the --force flag to skip confirmation prompts.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.ForceOption);
    }

    protected override ServerDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Force = parseResult.GetValueOrDefault<bool>(SqlOptionDefinitions.ForceOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Show warning about destructive operation unless force is specified
            if (!options.Force)
            {
                context.Response.Status = 200;
                context.Response.Message =
                    $"WARNING: This operation will permanently delete the SQL server '{options.Server}' " +
                    $"and ALL its databases in resource group '{options.ResourceGroup}'. " +
                    $"This action cannot be undone. Use --force to confirm deletion.";
                return context.Response;
            }

            var sqlService = context.GetService<ISqlService>();

            var deleted = await sqlService.DeleteServerAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            if (deleted)
            {
                context.Response.Results = ResponseResult.Create(new($"SQL server '{options.Server}' was successfully deleted.", true), SqlJsonContext.Default.ServerDeleteResult);
            }
            else
            {
                context.Response.Status = 404;
                context.Response.Message = $"SQL server '{options.Server}' not found in resource group '{options.ResourceGroup}'.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            $"The given SQL server not found. It may have already been deleted.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed deleting the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == 409 =>
            $"Cannot delete SQL server due to a conflict. It may be in use or have dependent resources. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

    internal record ServerDeleteResult(string Message, bool Success);
}
