// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Bicep;

[CommandMetadata(
    Id = "2231c968-af41-43a7-b387-59609bb5bfab",
    Name = "review",
    Title = "Review and correct existing Bicep for resilience",
    Description = """
        Use this command when the user asks to review, assess, or correct existing Bicep for
        availability-zone or zonal-resilience gaps. Do not review pasted Bicep independently of this
        command. Prefer file attachments for complete templates, especially when entry files, local
        modules, or .bicepparam files must be reviewed together. Prepare every relevant local file
        with resiliencyagent_file_attach, then pass all returned IDs through attachment-ids and
        describe their relationships in request. If the user pasted Bicep directly in the current
        prompt, pass that source verbatim in request without file attachment; do not create a
        temporary file solely to attach pasted source.
        Preserve exact observed names, values, references, scopes, and requirements.
        Use resiliencyagent_architecture_assess for an application design and
        resiliencyagent_iac_generate when no existing source needs review.
        The backend asks its own clarifying questions. Show them verbatim and do not answer on the
        user's behalf. Pass the returned conversationId on related follow-up calls.
        Corrected Bicep artifacts are final and authoritative. Present returned MCP file resources
        and use the host's native file-edit workflow when applying them to corresponding existing
        files. Do not independently run Bicep build, ARM validate/what-if, or another semantic
        validation against them, and do not rewrite them from local diagnostics. Request a backend
        change only when the user explicitly asks for it or the backend reports incomplete
        validation. Offer suggestedNextSteps when present.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class BicepReviewCommand(
    ILogger<BicepReviewCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache,
    TimeProvider? timeProvider = null)
    : ResiliencyAgentCommand<ResiliencyAgentAttachmentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache,
        timeProvider,
        artifactInstructions: """
            Corrected Bicep files are authoritative backend output. Do not independently run Bicep
            build, ARM validate/what-if, or another semantic validation against them. Do not patch
            them from local diagnostics. Apply and present them exactly as returned; if the backend
            reports incomplete validation, continue this backend conversation instead.
            """);
