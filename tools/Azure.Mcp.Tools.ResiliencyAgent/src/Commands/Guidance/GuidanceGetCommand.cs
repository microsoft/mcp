// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Guidance;

[CommandMetadata(
    Id = "bf57216c-2015-41ac-b83f-397e4c8cd62e",
    Name = "get",
    Title = "Get general Azure resiliency guidance",
    Description = """
        Use this command for general, conceptual questions about Azure resiliency, availability zones,
        zone resilience, zone redundancy, high availability, and how to begin a resiliency journey.
        This command provides education and guidance without assessing a specific application
        architecture, generating infrastructure-as-code, reviewing an existing template, or inspecting
        deployed Azure resource state. Pass the user's question in request without inventing an
        application architecture or adding resource-specific assumptions.
        Use resiliencyagent_architecture_assess when the user provides an application design that needs
        assessment, resiliencyagent_iac_generate for new infrastructure-as-code, and the format-specific
        review commands for existing templates.
        The backend guidance is authoritative. Show backend clarification questions verbatim, pass the
        returned conversationId on related follow-up calls, and offer suggestedNextSteps when present.
        """,
    OperationPlane = ToolOperationPlane.NotApplicable,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class GuidanceGetCommand(
    ILogger<GuidanceGetCommand> logger,
    IResiliencyAgentService service,
    IArtifactWriter artifactWriter,
    IAttachmentCache attachmentCache)
    : ResiliencyAgentCommand<ResiliencyAgentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache);
