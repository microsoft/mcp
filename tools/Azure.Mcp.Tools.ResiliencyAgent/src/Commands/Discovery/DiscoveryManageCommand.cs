// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Discovery;

[CommandMetadata(
    Id = "c89d0d7f-6062-4d43-bcd7-688e39d934f1",
    Name = "manage",
    Title = "Configure and run Discovery or edit arrangement plans",
    Description = """
        Create an Azure application Discovery Agent for a subscription after confirmation. Assign
        Reader access to a selected Discovery Agent only after confirmation. Run every fresh
        application discovery. Create an arrangement from a saved discovery. Rename and regroup the
        proposed automatic-Discovery Service Group arrangement, then show its updated preview. Merge
        or split groups, exclude resources, undo the last change to a saved Discovery arrangement,
        or revert arrangement changes.
        Undo or revert saved Discovery arrangement changes and restore the prior preview.
        Rename and regroup the proposed Service Group arrangement, then show the updated preview.
        Undo the last change to the saved Discovery arrangement.
        The backend owns validation and confirmation; present every question and never approve for
        the user. Discovery get owns agent listings and Reader checks. This command does not execute
        the final plan.
        A completed backend reply is authoritative and terminal: present it and stop. Never retry,
        act on suggestedNextSteps, choose a discovery reference, infer a fresh run, or call another
        Discovery tool without a new explicit user request. If saved discoveries are returned, wait
        for the user to select one or explicitly request a fresh discovery.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class DiscoveryManageCommand(
    ILogger<DiscoveryManageCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache,
    TimeProvider? timeProvider = null)
    : ResiliencyAgentCommand<ResiliencyAgentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache,
        timeProvider,
        artifactInstructions: """
            Backend-authored Discovery Markdown artifact content is authoritative. Preserve content
            exactly, including Portal-specific :::card and :::::accordion directives. Use the local
            filename and workspace link returned by Azure MCP. The writer may apply standard path,
            whitespace, control-character, and invalid-character sanitization; do not otherwise
            rename, transcode, rewrite, re-evaluate, reject, or replace the artifact.
            """);
