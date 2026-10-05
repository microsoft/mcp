// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Architecture;

[CommandMetadata(
    Id = "a25f9a6f-f739-4466-a10b-6294398a7169",
    Name = "assess",
    Title = "Assess an Azure application architecture for zonal resilience",
    Description = """
        Use this command when the user asks to assess an application design or architecture for
        availability-zone and zonal-resilience gaps before deployment. Describe the architecture in
        request and, when needed, include files prepared with resiliencyagent_file_attach through
        attachment-ids. This command does not require deployed resources and does not inspect or
        validate live Azure resource state.
        Include the exact observed Azure resource types, region, SKUs, dependencies, topology,
        requirements, and constraints. Label missing, ambiguous, inferred, or conflicting details as
        uncertain rather than guessing.
        The backend is authoritative and may produce a readable Markdown architecture assessment.
        Present returned MCP file resources to the user. Do not independently add Azure
        recommendations or rewrite, extend, improve, correct, or ask for modifications to any
        backend-generated report. Request backend changes only when the user explicitly asks.
        Do not use generic Azure service or well-architected guidance tools for this application-level
        zonal assessment. Use resiliencyagent_iac_generate for new infrastructure-as-code and the
        format-specific review commands for existing templates.
        Show backend clarifying questions verbatim, pass conversationId on related follow-up calls,
        and offer suggestedNextSteps when present.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class ArchitectureAssessCommand(
    ILogger<ArchitectureAssessCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache)
    : ResiliencyAgentCommand<ResiliencyAgentAttachmentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache);
