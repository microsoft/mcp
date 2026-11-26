// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Options;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResiliencyAgent.Commands.Drills;

[CommandMetadata(
    Id = "ad88c78f-4649-4a60-86a4-12d8df11d249",
    Name = "get",
    Title = "Inspect and check Azure resilience drills",
    Description = """
        Use the conversational Stay Resilient drills workflow to inspect, compare, or diagnose
        zone-down drills on Azure Service Groups. It can list drills and their resources or runs,
        explain dependencies and readiness blockers, inspect monitoring and Recovery Plan usage,
        compare drills across visible Service Groups, and summarize run or resource failures.
        It can also run drill resync-readiness and execution-validation checks on request. These
        checks may submit backend assessment jobs, but they do not start or complete a drill run.
        Select this agent-backed command when interpretation, lifecycle guidance, aggregation,
        clarification, prerequisite discovery, complete lists, or evidence-backed diagnosis is the
        outcome. Forward exact Service Group, drill, run, resource, location, and operation values
        already supplied. Requests outside the supported chat action surface receive authoritative
        Azure Portal and Microsoft Learn guidance rather than an inferred substitute operation.
        Use resiliencyagent_drills_manage for confirmed drill creation or configuration changes.
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
public sealed class DrillsGetCommand(
    ILogger<DrillsGetCommand> logger,
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
            A returned FullList.md file is the authoritative complete drill, resource, run, job, or
            readiness list behind the response summary. Preserve every row, count, status, warning,
            ordering choice, and Azure Portal link. Do not replace it with a shorter reconstruction.
            """);
