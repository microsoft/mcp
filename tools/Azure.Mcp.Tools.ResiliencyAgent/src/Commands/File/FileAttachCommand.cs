// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Commands;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.File;

[CommandMetadata(
    Id = "5dd3e41e-7194-4136-93f4-504a7beca6e5",
    Name = "attach",
    Title = "Prepare a local file for Azure Resiliency analysis",
    Description = """
        Prepare exactly one user-selected local architecture or infrastructure-as-code file for a
        later Azure Resiliency scenario call. Supply its fully-qualified path. The command validates
        the path, type, and ACP-aligned 1.4 MiB raw-size limit, then shows the exact path, file type,
        size, and hosted Azure Resiliency Agent destination in a user confirmation before reading any
        bytes. If approved, it stores a short-lived process-local immutable snapshot and returns an
        opaque attachmentId. Pass that ID to architecture_assess, bicep_review, arm_review, or
        terraform_review. This command does not contact A2A and never returns file content or Base64.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class FileAttachCommand(
    ILogger<FileAttachCommand> logger,
    ILocalFileSnapshotter snapshotter,
    IAttachmentCache attachmentCache)
    : BaseCommand<FileAttachOptions, FileAttachmentResult>
{
    private const string ApprovalField = "approve";
    private readonly ILogger<FileAttachCommand> _logger = logger;

    /// <summary>
    /// Validates metadata, obtains explicit user consent, snapshots the file, and returns only an
    /// opaque handle and non-content metadata.
    /// </summary>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        FileAttachOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidatedLocalFile file = snapshotter.Validate(options.FilePath);
            await RequireApprovalAsync(context, file, cancellationToken);

            AgentAttachment snapshot = await snapshotter.SnapshotAsync(file, cancellationToken);
            attachmentCache.Add(snapshot);

            context.Response.Results = ResponseResult.Create(
                new FileAttachmentResult(
                    snapshot.AttachmentId,
                    snapshot.Name,
                    snapshot.MimeType,
                    snapshot.Content.LongLength,
                    snapshot.Sha256,
                    snapshot.ExpiresAt),
                ResiliencyAgentJsonContext.Default.FileAttachmentResult);
        }
        catch (OperationCanceledException)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The file attachment was cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not prepare the selected local file.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// Requires a client-rendered confirmation that displays the canonical path, detected type,
    /// raw size, remote destination, and short-lived local caching behavior before bytes are read.
    /// </summary>
    private static async Task RequireApprovalAsync(
        CommandContext context,
        ValidatedLocalFile file,
        CancellationToken cancellationToken)
    {
        McpServer? server = context.McpServer;
        if (server is null || !server.SupportsElicitation())
        {
            throw new InvalidOperationException(
                "This client cannot request the user confirmation required before reading a local file.");
        }

        ElicitResult result = await server.ElicitAsync(
            new ElicitRequestParams
            {
                Message =
                    "Share this local file with the hosted Azure Resiliency Agent?\n\n" +
                    $"File: {file.CanonicalPath}\n" +
                    $"Type: {file.MimeType}\n" +
                    $"Size: {file.SizeBytes:N0} bytes\n\n" +
                    "Approval allows the local Azure MCP process to read the file and retain a " +
                    "short-lived in-memory snapshot. The file is sent only when a later Azure " +
                    "Resiliency scenario call uses the returned attachment ID.",
                RequestedSchema = new ElicitRequestParams.RequestSchema
                {
                    Properties = new Dictionary<string, ElicitRequestParams.PrimitiveSchemaDefinition>
                    {
                        [ApprovalField] = new ElicitRequestParams.BooleanSchema
                        {
                            Title = "Share this file",
                            Description = "Approve reading and preparing the exact file shown above.",
                            Default = false,
                        },
                    },
                    Required = [ApprovalField],
                },
            },
            cancellationToken);

        if (!result.IsAccepted
            || result.Content is null
            || !result.Content.TryGetValue(ApprovalField, out JsonElement approved)
            || approved.ValueKind != JsonValueKind.True)
        {
            throw new OperationCanceledException("The user did not approve sharing the selected file.");
        }
    }
}
