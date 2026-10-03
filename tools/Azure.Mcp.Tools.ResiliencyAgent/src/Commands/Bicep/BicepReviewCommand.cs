// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Bicep;

[CommandMetadata(
    Id = "2231c968-af41-43a7-b387-59609bb5bfab",
    Name = "review",
    Title = "Review and correct existing Bicep for resilience",
    Description = """
        Always use this Start Resilient command to review or correct existing Bicep; do not review
        pasted source independently. Prefer file attachments for complete templates, especially when
        entry files, local modules, or parameter files must be reviewed together. Prepare every
        relevant local file with
        resiliencyagent_file_attach, then pass all returned IDs through attachment-ids and describe
        their relationships in request. If the user instead pasted Bicep directly in the current
        prompt, pass that source verbatim in request and call this command without file attachment;
        do not create a temporary file solely to attach pasted source.
        Preserve exact observed names, values, references, scopes, and requirements.
        Use resiliencyagent_architecture_assess for an application design and
        resiliencyagent_iac_generate when no existing source needs review.
        The backend asks its own clarifying questions. Show them verbatim and do not answer on the
        user's behalf. Pass the returned conversationId on related follow-up calls.
        Corrected Bicep artifacts are final and authoritative. Present returned MCP file resources
        and use the host's native file-edit workflow when applying them to corresponding existing
        files. Do not independently rewrite their resilience-specific content. Request a backend
        change only when the user explicitly asks for it. Offer suggestedNextSteps when present.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class BicepReviewCommand(
    ILogger<BicepReviewCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache)
    : ResiliencyAgentCommand<ResiliencyAgentAttachmentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache);
