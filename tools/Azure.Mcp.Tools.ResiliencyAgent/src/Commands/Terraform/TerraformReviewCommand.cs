// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Terraform;

[CommandMetadata(
    Id = "6b52d844-d4fd-4e60-94bb-587b19aa2a31",
    Name = "review",
    Title = "Review and correct existing Terraform for resilience",
    Description = """
        Always use this Start Resilient command to review or correct existing Terraform; do not
        review pasted source independently. Prefer file attachments for complete configurations,
        especially when root modules, local child modules, variables files, or tfvars files must be
        reviewed together. Prepare every relevant local file with resiliencyagent_file_attach, then
        pass all returned IDs through attachment-ids and describe module relationships in request. If
        the user instead pasted Terraform directly in the current prompt, pass it verbatim in request
        and call this command without file attachment; do not create a temporary file solely to
        attach pasted source. Preserve provider constraints, resource addresses, variable values,
        dependencies, regions, names, and requirements.
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
public sealed class TerraformReviewCommand(
    ILogger<TerraformReviewCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache)
    : ResiliencyAgentCommand<ResiliencyAgentAttachmentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache);
