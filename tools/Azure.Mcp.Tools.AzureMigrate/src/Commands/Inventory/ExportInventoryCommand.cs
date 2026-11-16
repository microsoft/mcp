// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureMigrate.Options.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;

/// <summary>
/// POC: returns a read-only download link for the processed output of a finished inventory with one call to the
/// Discovery Service (POST /mcprestapi/sessions/{id}/export), forwarding the caller's bearer token unchanged.
/// </summary>
[CommandMetadata(
    Id = "88304b20-dbf6-4c54-8202-c7a6613b0bb7",
    Name = "export",
    Title = "Export Azure Migrate Inventory",
    Description = "Gets a download link for the processed output file of a finished Azure Migrate inventory. Call this when the user " +
        "asks to download, export or get the file of their inventory, after the inventory status tool reports it as completed. " +
        "Requires 'session-id' from the latest create-or-update result in the conversation (ask the user to start an inventory if " +
        "there is none). Returns 'downloadUrl', 'downloadCommand' (an azcopy command) and 'instructions'. Follow 'instructions'. " +
        "Do not mention tool or parameter names to the user.",
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ExportInventoryCommand(ILogger<ExportInventoryCommand> logger, IHttpContextAccessor httpContextAccessor)
    : BaseCommand<ExportInventoryOptions, ExportInventoryCommand.ExportInventoryCommandResult>
{
    internal const string FilePathPlaceholder = "<path to save the file>";

    private readonly ILogger<ExportInventoryCommand> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ExportInventoryOptions options, CancellationToken cancellationToken)
    {
        ExportInventoryCommandResult result = await RunAsync(DsBackdoorClient.BaseUrl, options, cancellationToken);

        context.Response.Status = HttpStatusCode.OK;
        context.Response.Results = ResponseResult.Create(result, AzureMigrateJsonContext.Default.ExportInventoryCommandResult);
        context.Response.Message = string.Empty;
        return context.Response;
    }

    private async Task<ExportInventoryCommandResult> RunAsync(string baseUrl, ExportInventoryOptions options, CancellationToken cancellationToken)
    {
        string? sessionId = options.SessionId?.Trim();
        if (!DsBackdoorClient.IsValidSessionId(sessionId))
        {
            return Fail(sessionId, null, "'session-id' is required: 1-20 characters using only letters, digits, '-' or '_'.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(sessionId, null, authError);
        }

        DsBackdoorClient.DsResponse response = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Post, DsBackdoorClient.ExportUrl(baseUrl, sessionId!), token, null,
            Guid.NewGuid().ToString(), "export", cancellationToken);
        return Parse(sessionId!, response);
    }

    internal static ExportInventoryCommandResult Parse(string sessionId, DsBackdoorClient.DsResponse response)
    {
        if (response.Status == (int)HttpStatusCode.NotFound)
        {
            return Fail(sessionId, response.Status, "No inventory was found for this session ID for the signed-in user.",
                "Tell the user no inventory was found for them with this ID and offer to start a new one.");
        }

        if (response.Status == (int)HttpStatusCode.BadRequest)
        {
            return Fail(sessionId, response.Status, response.Error,
                "Tell the user the inventory output is not available yet: the inventory may still be running or produced no file. " +
                "Suggest checking the inventory status first.");
        }

        if (response.Status is not (>= 200 and < 300))
        {
            return Fail(sessionId, response.Status, response.Error);
        }

        string? downloadUrl = DsBackdoorClient.ReadValue(response.Body, "uri");
        string? downloadCommand = BuildDownloadCommand(downloadUrl);
        if (downloadCommand is null)
        {
            return Fail(sessionId, response.Status, "The Discovery Service response did not include a valid download URL.");
        }

        return new(
            Success: true,
            Error: null,
            HttpStatus: response.Status,
            SessionId: sessionId,
            DownloadUrl: downloadUrl,
            DownloadCommand: downloadCommand,
            Instructions: "Tell the user their inventory output is ready to download. Share downloadUrl as a clickable link exactly as " +
                "returned, without retyping or shortening it, and mention that the link is temporary and read-only. If they prefer the " +
                $"command line, show downloadCommand exactly as returned in a code block and ask them to replace {FilePathPlaceholder} " +
                "with where to save the file (AzCopy v10 is required: https://aka.ms/downloadazcopy-v10-windows). If the link has " +
                "expired, they can ask for a new one.");
    }

    /// <summary>
    /// Builds the azcopy command for a read-only blob SAS URL, or returns null if the URL is not a valid http(s) URL.
    /// </summary>
    internal static string? BuildDownloadCommand(string? downloadUrl)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl)
            || !Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri? uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            || downloadUrl.Contains('"'))
        {
            return null;
        }

        return $"azcopy copy \"{downloadUrl}\" \"{FilePathPlaceholder}\" --from-to=BlobLocal";
    }

    private static ExportInventoryCommandResult Fail(string? sessionId, int? status, string? error, string? instructions = null) =>
        new(false, error ?? "Unknown error.", status, sessionId, null, null, instructions);

    /// <summary>Result of the inventory export command.</summary>
    public sealed record ExportInventoryCommandResult(
        bool Success,
        string? Error,
        int? HttpStatus,
        string? SessionId,
        string? DownloadUrl,
        string? DownloadCommand,
        string? Instructions);
}
