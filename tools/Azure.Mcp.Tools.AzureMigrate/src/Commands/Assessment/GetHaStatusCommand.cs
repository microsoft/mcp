// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;
using Azure.Mcp.Tools.AzureMigrate.Options.Assessment;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// POC: reads the Discovery Service operation status of the HA session started by the assessment create command
/// (GET /mcprestapi/sessions/{id}/operationstatus/{operationId}), forwarding the caller's bearer token unchanged.
/// It does not wait or poll.
/// </summary>
[CommandMetadata(
    Id = "43da1c44-2cef-46e3-872d-49e93b47bd4b",
    Name = "hastatus",
    Title = "Get Azure Migrate HA Assessment Status",
    Description = "Gets the current status of the HA assessment started by the assessment create tool. Call this once when the user " +
        "asks about the progress or result of their HA assessment; do not loop or wait. Requires 'ha-session-id' and 'ha-operation-id' " +
        "from the latest assessment create result in the conversation (ask the user to create an assessment if there is none). Returns " +
        "'status' (for example Running, Succeeded or Failed), 'isCompleted', 'instructions', and when finished either 'result' or " +
        "'errorCode'/'errorMessage'. Follow 'instructions'. Summarise 'result' in plain language without inventing details. " +
        "Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class GetHaStatusCommand(ILogger<GetHaStatusCommand> logger, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<GetHaStatusOptions, GetInventoryStatusCommand.GetInventoryStatusCommandResult>
{
    internal const string Subject = "HA assessment";

    private readonly ILogger<GetHaStatusCommand> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GetHaStatusOptions options, CancellationToken cancellationToken)
    {
        GetInventoryStatusCommand.GetInventoryStatusCommandResult result = await RunAsync(DsBackdoorClient.BaseUrl, options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.GetInventoryStatusCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    private async Task<GetInventoryStatusCommand.GetInventoryStatusCommandResult> RunAsync(
        string baseUrl, GetHaStatusOptions options, CancellationToken cancellationToken)
    {
        string? sessionId = options.HaSessionId?.Trim();
        string? operationIdText = options.HaOperationId?.Trim();
        if (!DsBackdoorClient.IsValidSessionId(sessionId))
        {
            return Fail(sessionId, operationIdText, "'ha-session-id' is required: 1-20 characters using only letters, digits, '-' or '_'.");
        }

        if (!Guid.TryParse(operationIdText, out Guid operationId))
        {
            return Fail(sessionId, operationIdText, "'ha-operation-id' is required and must be a GUID.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(sessionId, operationIdText, authError);
        }

        if (MockHaSessionClient.IsMockSession(sessionId))
        {
            return new(true, null, (int)HttpStatusCode.OK, sessionId, operationId.ToString("D"), "Succeeded", true,
                "Mocked HA result: the HA skill is not deployed yet, so no HA assessment was run.", null, null, null,
                "Tell the user the HA assessment is mocked because the HA skill is not deployed yet, so there is no HA result to show.");
        }

        DsBackdoorClient.DsResponse response = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Get, DsBackdoorClient.OperationStatusUrl(baseUrl, sessionId!, operationId), token, null,
            Guid.NewGuid().ToString(), "get HA operation status", cancellationToken);
        return GetInventoryStatusCommand.Parse(sessionId!, operationId.ToString("D"), response, Subject);
    }

    private static GetInventoryStatusCommand.GetInventoryStatusCommandResult Fail(string? sessionId, string? operationId, string? error) =>
        new(false, error ?? "Unknown error.", null, sessionId, operationId, null, false, null, null, null, null, null);
}
