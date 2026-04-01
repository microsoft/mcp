// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.RecoveryPoint;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.RecoveryPoint;

/// <summary>
/// Consolidated recovery point command: when --recovery-point is supplied returns a single
/// recovery point's details; otherwise lists all recovery points for the protected item.
/// </summary>
public sealed class RecoveryPointGetCommand(ILogger<RecoveryPointGetCommand> logger) : BaseProtectedItemCommand<RecoveryPointGetOptions>()
{
    private const string CommandTitle = "Get Recovery Point";
    private readonly ILogger<RecoveryPointGetCommand> _logger = logger;

    public override string Id => "e930bbb6-b495-454b-bae4-46b9da14eb1c";
    public override string Name => "get";
    public override string Description =>
        """
        Retrieves recovery point information for a protected item. When --recovery-point is
        specified, returns detailed information about a single recovery point including time
        and type. When omitted, lists all available recovery points for the protected item.
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.RecoveryPoint.AsOptional());
    }

    protected override RecoveryPointGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.RecoveryPoint = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.RecoveryPoint.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var service = context.GetService<IAzureBackupService>();

            if (!string.IsNullOrEmpty(options.RecoveryPoint))
            {
                var rp = await service.GetRecoveryPointAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.ProtectedItem!,
                    options.RecoveryPoint,
                    options.VaultType,
                    options.Container,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new RecoveryPointGetCommandResult([rp]),
                    AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);
            }
            else
            {
                var points = await service.ListRecoveryPointsAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.ProtectedItem!,
                    options.VaultType,
                    options.Container,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new RecoveryPointGetCommandResult(points),
                    AzureBackupJsonContext.Default.RecoveryPointGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recovery point(s). RecoveryPoint: {RecoveryPoint}, Vault: {Vault}",
                options.RecoveryPoint, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery point not found. Verify the recovery point ID and protected item.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record RecoveryPointGetCommandResult([property: JsonPropertyName("recoveryPoints")] List<RecoveryPointInfo> RecoveryPoints);
}
