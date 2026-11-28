// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.ZonalResiliency;

[CommandMetadata(
    Id = "3f6ab4dc-9c17-4b1b-a688-1418a422069a",
    Name = "get",
    Title = "Check Azure zonal posture, prerequisites, and blockers",
    Description = """
        Check zonal-resiliency prerequisites and blocking conditions for existing Azure resources
        without making changes. Also use this read and assessment command for deployed Azure
        zonal-resiliency posture and existing-resource eligibility. It can summarize or detail one
        Service Group, compare visible Service Groups, inspect specific resources, resource types,
        resource groups, or subscriptions, and answer eligibility questions such as whether
        zone-redundant high availability can be enabled on supported existing resources. Use it for
        zonal-resiliency prerequisites, readiness,
        blockers, and "can I enable zone redundancy" questions.
        Include the exact Service Group name, resource ID, resource name, resource type, subscription,
        or resource-group scope available from the user's request. The backend validates scope and
        owns all supported-resource rules.
        Returned posture, prerequisite, and Service Group assessment artifacts are authoritative.
        Present every returned Markdown file and preserve resource and Azure Portal links. Continue
        related turns with conversationId.
        Do not use this command for changing Azure resources or generating scripts. Use
        resiliencyagent_zonalresiliency_manage for confirmed direct enablement and
        resiliencyagent_zonalresiliency_generate for scripts or templates. Use the general guidance
        command for conceptual, cost, migration, limitation, or Portal-how-to questions.
        A completed backend reply is authoritative and terminal: present it and stop. Never retry,
        act on suggestedNextSteps, infer another operation, or call another Resiliency Agent tool
        without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class ZonalResiliencyGetCommand(
    ILogger<ZonalResiliencyGetCommand> logger,
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
