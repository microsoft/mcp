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
        Use this Start Resilient command only to review and correct existing ARM JSON templates.
        When ARM JSON is supplied directly in the user's prompt, pass it verbatim in request and
        call this command without file attachment. For source in local files, prepare every relevant
        template, nested deployment, linked local file, and parameter file with
        resiliencyagent_file_attach, then pass all returned IDs through attachment-ids and describe
        their relationships in request. Preserve exact resource names, API versions, parameters,
        dependencies, scopes, and requirements.
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
    ReadOnly = true,
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
