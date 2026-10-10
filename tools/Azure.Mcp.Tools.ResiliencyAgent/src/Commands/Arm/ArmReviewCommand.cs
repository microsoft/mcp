// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Arm;

[CommandMetadata(
    Id = "d470ef09-df36-4378-bdda-bf032b40934c",
    Name = "review",
    Title = "Review and correct existing ARM JSON for resilience",
    Description = """
        Use this command when the user asks to review, assess, or correct an existing ARM JSON
        template for availability-zone or zonal-resilience gaps. Do not review pasted ARM JSON
        independently of this command. Prefer file attachments for complete templates, especially
        when templates, nested deployments, linked local files, or parameter files must be reviewed
        together. Prepare every relevant local file with resiliencyagent_file_attach, then pass all
        returned IDs through attachment-ids and describe their relationships in request. If the user
        pasted ARM JSON directly in the current prompt, pass it verbatim in request without file
        attachment; do not create a temporary file solely to attach pasted source.
        Preserve exact resource names, API versions, parameters, dependencies, scopes, and
        requirements.
        Use resiliencyagent_architecture_assess for an application design and
        resiliencyagent_iac_generate when no existing source needs review.
        Corrected artifacts are authoritative. Do not independently rewrite their
        resilience-specific content. Show backend clarification questions verbatim, pass the returned
        conversationId on related follow-ups, and offer suggestedNextSteps when present.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class ArmReviewCommand(
    ILogger<ArmReviewCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache)
    : ResiliencyAgentCommand<ResiliencyAgentAttachmentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache);
