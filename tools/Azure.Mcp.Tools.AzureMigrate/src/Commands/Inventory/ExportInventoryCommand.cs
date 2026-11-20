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
    Title = "Export Azure Catalyst Inventory",
    Description = "Gets a download link for an output of a finished Azure Catalyst inventory. " +
        "When: the user asks to download, export or get the file of their inventory, after the inventory status tool reports it " +
        "as completed. " +
        "How: pass 'session-id' from the latest create-or-update result in the conversation; ask the user to start an inventory if " +
        "there is none. Set 'output' to 'refined' for the refined (cleaned-up) inventory file, which is the default, or to " +
        "'offline' for the offline zip package; ask the user if it is unclear which one they want. " +
        "Returns: 'downloadUrl', 'downloadCommand' (an azcopy command) and 'instructions'. " +
        "Next: share the download link or command with the user. " +
        "Follow 'instructions'. Do not mention tool or parameter names to the user.",
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
    internal const string RefinedOutput = "refined";
    internal const string OfflineOutput = "offline";

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
            return Fail(sessionId, null, null, "'session-id' is required: 1-20 characters using only letters, digits, '-' or '_'.");
        }

        string? output = NormalizeOutput(options.Output);
        if (output is null)
        {
            return Fail(sessionId, null, null, $"'output' must be '{RefinedOutput}' or '{OfflineOutput}'.");
        }

        (string? token, string? authError) = DsBackdoorClient.GetBearerToken(_httpContextAccessor);
        if (token is null)
        {
            return Fail(sessionId, output, null, authError);
        }

        DsBackdoorClient.DsResponse response = await DsBackdoorClient.SendAsync(
            _logger, HttpMethod.Post, DsBackdoorClient.ExportUrl(baseUrl, sessionId!, ToArtifacts(output)), token, null,
            Guid.NewGuid().ToString(), "export", cancellationToken);
        return Parse(sessionId!, output, response);
    }

    /// <summary>
    /// Returns 'refined' or 'offline' (default 'refined'), or null if the value is not recognised.
    /// </summary>
    internal static string? NormalizeOutput(string? output)
    {
        if (string.IsNullOrWhiteSpace(output) || string.Equals(output.Trim(), RefinedOutput, StringComparison.OrdinalIgnoreCase))
        {
            return RefinedOutput;
        }

        return string.Equals(output.Trim(), OfflineOutput, StringComparison.OrdinalIgnoreCase) ? OfflineOutput : null;
    }

    /// <summary>
    /// Maps the user-facing output to the Discovery Service 'artifacts' value: refined is the external output, offline the internal one.
    /// </summary>
    internal static string ToArtifacts(string output) => output == OfflineOutput ? "internal" : "external";

    internal static ExportInventoryCommandResult Parse(string sessionId, string output, DsBackdoorClient.DsResponse response)
    {
        string label = output == OfflineOutput ? "offline zip package" : "refined inventory file";
        if (response.Status == (int)HttpStatusCode.NotFound)
        {
            return Fail(sessionId, output, response.Status, "No inventory was found for this session ID for the signed-in user.",
                "Tell the user no inventory was found for them with this ID and offer to start a new one.");
        }

        if (response.Status == (int)HttpStatusCode.BadRequest)
        {
            return Fail(sessionId, output, response.Status, response.Error,
                $"Tell the user the {label} is not available yet: the inventory may still be running or did not produce it. " +
                "Suggest checking the inventory status first.");
        }

        if (response.Status is not (>= 200 and < 300))
        {
            return Fail(sessionId, output, response.Status, response.Error);
        }

        string? downloadUrl = DsBackdoorClient.ReadValue(response.Body, "uri");
        string? downloadCommand = BuildDownloadCommand(downloadUrl);
        if (downloadCommand is null)
        {
            return Fail(sessionId, output, response.Status, "The Discovery Service response did not include a valid download URL.");
        }

        return new(
            Success: true,
            Error: null,
            HttpStatus: response.Status,
            SessionId: sessionId,
            Output: output,
            DownloadUrl: downloadUrl,
            DownloadCommand: downloadCommand,
            Instructions: $"Tell the user their {label} is ready to download. Share downloadUrl as a clickable link exactly as " +
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

    private static ExportInventoryCommandResult Fail(string? sessionId, string? output, int? status, string? error, string? instructions = null) =>
        new(false, error ?? "Unknown error.", status, sessionId, output, null, null, instructions);

    /// <summary>Result of the inventory export command.</summary>
    public sealed record ExportInventoryCommandResult(
        bool Success,
        string? Error,
        int? HttpStatus,
        string? SessionId,
        string? Output,
        string? DownloadUrl,
        string? DownloadCommand,
        string? Instructions);
}
