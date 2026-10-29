// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Iac;

[CommandMetadata(
    Id = "13caf95e-b18e-4f7b-a70d-b2bf4456a0ee",
    Name = "generate",
    Title = "Generate new resilient infrastructure as code",
    Description = """
        Use this command when the user asks to create or generate new zone-resilient Bicep, ARM JSON,
        or Terraform for an application described in request. This command does not review or correct
        existing files and does not accept attachments. Preserve the user's requested architecture,
        resources, regions, naming constraints, relationships, and target format. If the requested
        IaC format is ambiguous, let the backend ask whether to generate Bicep, ARM JSON, or Terraform.
        Use resiliencyagent_architecture_assess for assessment without generation, and use the
        format-specific review commands when existing source must be corrected.
        Backend-generated files are authoritative and a completed result has already passed the
        backend's format-appropriate live validation. Do not run independent Bicep builds, ARM
        validate/what-if calls, Terraform validate/plan operations, or other semantic validation
        against returned files, and do not rewrite them based on local diagnostics. Apply and present
        them exactly as returned. If the backend explicitly reports that validation did not complete,
        continue the same backend conversation rather than substituting local validation. Show backend
        clarification questions verbatim, pass the returned conversationId on related follow-ups, and
        offer suggestedNextSteps when present.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class IacGenerateCommand(
    ILogger<IacGenerateCommand> logger,
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
            The returned infrastructure-as-code files are authoritative backend output and have already
            completed the Azure Resiliency Agent's format-appropriate live validation. Do not run Bicep
            build, ARM validate/what-if, Terraform validate/plan, or another independent semantic
            validation against them. Do not patch or regenerate them from local diagnostics. Apply and
            present the files exactly as returned. If the backend reply explicitly says validation was
            not completed, continue this conversation with the backend instead of validating locally.
            """);
