// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.ZonalResiliency;

[CommandMetadata(
    Id = "b66ceb8f-ca22-429f-bdb3-e7962a1e6618",
    Name = "manage",
    Title = "Apply Azure zone-redundancy configuration changes",
    Description = """
        Directly execute an explicitly requested zone-redundancy configuration change on a supported
        existing Azure resource. Use only when the intended outcome is a changed deployed resource
        now. Include the exact resource scope and requested change. The backend validates scope and
        prerequisites, asks for explicit user confirmation, performs the approved mutation, refreshes
        the deployed posture, and verifies the outcome. Show every backend elicitation question
        verbatim and never approve a mutation for the user. Continue related turns with
        conversationId and present every returned authoritative Markdown artifact.
        A completed, declined, or cancelled backend reply is authoritative and terminal: present it
        and stop. Never retry the mutation, act on suggestedNextSteps, infer another operation, or
        call another Resiliency Agent tool without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class ZonalResiliencyManageCommand(
    ILogger<ZonalResiliencyManageCommand> logger,
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
            The returned Markdown files are authoritative Azure Resiliency output. Do not independently
            regenerate, reinterpret, or alter their content. Apply and present the files exactly as
            returned.
            """);
