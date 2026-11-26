// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.RecoveryPlans;

[CommandMetadata(
    Id = "4f25358d-70f3-4e6d-9308-4298782109d6",
    Name = "get",
    Title = "Compare Azure Recovery Plans and analyze recovery history",
    Description = """
        Use the conversational Stay Resilient Recovery Plans workflow for cross-plan comparison,
        complete recovery-history analysis, or diagnosis that requires correlating multiple plan
        records. It can combine plans, resources, protection choices, recovery groups, jobs, and
        drills that use a plan; analyze job or resource failures; explain recovery order and plan
        blockers; and compare plans across visible Service Groups. Select this agent-backed command
        for broad interpretation, aggregation, prerequisite discovery, complete lists, or
        evidence-backed diagnosis. Forward exact Service Group, plan, job, resource, and scope values
        already supplied.
        Requests outside the supported chat action surface receive authoritative Azure Portal and
        Microsoft Learn guidance rather than an inferred substitute.
        Use resiliencyagent_recoveryplans_manage for confirmed plan creation or configuration
        changes.
        A completed, failed, rejected, or cancelled backend reply is authoritative and terminal:
        present authoritativeUserResponse verbatim and stop. Never act on suggestedNextSteps or
        call another Resiliency Agent tool without a new explicit user request.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class RecoveryPlansGetCommand(
    ILogger<RecoveryPlansGetCommand> logger,
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
            A returned FullList.md file is the authoritative complete Recovery Plan, resource, job,
            readiness, or validation list behind the response summary. Preserve every row, recovery
            group, status, warning, ordering choice, and Azure Portal link. Do not replace it with a
            shorter reconstruction.
            """);
