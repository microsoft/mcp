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
        Always use this authoritative Azure resiliency guidance command when the user asks to define,
        explain, or compare resiliency, reliability, availability zones, zone resilience, zone
        redundancy, high availability, or how to begin improving resiliency. Also use it for
        resource-type knowledge such as supported migration approaches, prerequisites, limitations,
        cost or downtime implications, SLA information, monitoring, RBAC, troubleshooting, and Azure
        Portal configuration steps. Prefer it over generic documentation search, well-architected
        guidance, and service-specific tools for these Azure resiliency questions. It provides
        education and next-step guidance without assessing a specific application architecture or
        deployed resource state, changing Azure, or generating scripts. Pass the user's question in
        request without inventing an application architecture or adding unsupported assumptions.
        Use resiliencyagent_zonalresiliency_get for deployed posture or enablement readiness,
        resiliencyagent_zonalresiliency_generate for existing-resource remediation scripts,
        resiliencyagent_architecture_assess when the user provides an application design that needs
        assessment, resiliencyagent_iac_generate for application infrastructure-as-code, and the
        format-specific review commands for existing templates.
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
    IAttachmentCache attachmentCache,
    TimeProvider? timeProvider = null)
    : ResiliencyAgentCommand<ResiliencyAgentRequestOptions>(
        logger,
        service,
        artifactWriter,
        attachmentCache,
        timeProvider);
